using System.Collections;
using System.Collections.Generic;
using MyUtilities;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles all logic for a Monster (a hostile unit to the player), including AI,
/// ability casting, and more.
/// </summary>
public class Monster : Unit
{
    public enum HealthBarType
    {
        Standard,
        Empowered,
        Boss
    }

    public UnitSpawnEffect spawnEffect;

    [Header("Monster Properties")]
    public float goldModifier = 1.0f;
    public float experienceModifier = 1.0f;
    public float corpseDuration = 5.0f;
    public string monsterLabel;
    [HideInInspector] public bool isEmpowered = false;

    [Header("Health Bar Properties")]
    public HealthBarType healthBarType = HealthBarType.Standard;
    public float uiSize = 1.0f;
    public float uiHeight = 2.0f;

    [Header("Spawn Data")]
    [Range(1, 10)] public int spawnLevel = 1;
    [Range(0, 10)] public int spawnLikelihood = 5;

    private const float StopDistanceBuffer = 0.1f;
    private const float MaximumPossibleRange = 10f;
    private const float DamageNumberSpawnVariance = 0.3f;
    private const float ActionDelayAfterSpawning = 1.0f;
    private const float EmpoweredScaleModifier = 1.5f;
    private const float EmpoweredUISizeModifier = 2.5f;
    private const float EmpoweredUIHeightModifier = 1.75f;
    private const float CriticalDamageNumberScaleMod = 1.5f;

    [SerializeField] Color OutlineColor = new Color(0.0f, 0.8f, 0.0f);
    private static readonly Color HitDamageTextColor = new Color(1, 1, 0.5f);
    private static readonly Color CritDamageTextColor = Color.red;

    // Cached components
    protected AudioSource source;


    // Flee State Variables 
    protected float zigzagDistance = 5f;       // Distance between each zigzag point
    protected float zigzagOffset = 6f;         // Maximum lateral offset for zigzagging
    protected int zigzagPointsCount = 3;       // Number of zigzag points to generate
    protected Vector3[] zigzagPoints;
    protected int currentPointIndex = 0;



    /// <summary>
    /// Performs all initial monster setup.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        CacheComponents();
        CalculateTargetRange();
        ApplyMonsterScaling();
        CreateHealthBar();
        ApplySpawnDelay();
        SetInitialFacing();
        SetOutline(OutlineColor, 3, 0.06f);
        if (source != null) source.Play();
    }

    /// <summary>
    /// Handles all frame-by-frame updates.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        if (GameManager.player.isDead || !unitIsActive) return;

        UpdateOutline();
        CalculateMonsterTargeting();

        if (engagementState == EngagementState.StandStill)
        {
            agentNavigation.SetDestination(this.transform.position);
            return;
        }          
        if (engagementState == EngagementState.MoveTowards)
            HandleMovement();
        else if (engagementState == EngagementState.Flee)
            HandleFleeMovement();
    }

    private void OnDrawGizmos()
    {
        #region Movment - Fleeing
        if (zigzagPoints == null || zigzagPoints.Length == 0) return;

        // Set the color of the gizmos (debug spheres)
        Gizmos.color = Color.red;

        // Loop through each point in the array
        foreach (Vector3 point in zigzagPoints)
        {
            // Draw a sphere at each point
            Gizmos.DrawSphere(point, 0.5f);
        }
        #endregion
    }


    /// <summary>
    /// Caches references to all required components.
    /// </summary>
    private void CacheComponents()
    {
        source = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Applies health and damage scaling to the monster.
    /// </summary>
    private void ApplyMonsterScaling()
    {
        float modifier = Mathf.Pow(GameManager.monsterScalingValues.increasedHealthPerPlayerLevel + 1, GameManager.player.currentLevel);

        baseMaxHealth *= modifier;
        baseDamage *= modifier;
    }

    /// <summary>
    /// Determines the initial facing of the monster (e.g., face towards the player).
    /// </summary>
    private void SetInitialFacing()
    {
        Vector3 diff = (GameManager.player.transform.position - transform.position);
        diff.y = 0;
        diff.Normalize();
        transform.LookAt(transform.position + diff);
    }

    /// <summary>
    /// Applies a preset spawn delay to the unit, preventing it from moving and 
    /// attacking immediately after spawning.
    /// </summary>
    private void ApplySpawnDelay()
    {
        canMoveAt = Time.time + ActionDelayAfterSpawning;
        canCastAt = Time.time + ActionDelayAfterSpawning;
    }

    /// <summary>
    /// Creates a functional health bar for the unit, which will keep track of
    /// important information.
    /// </summary>
    private void CreateHealthBar()
    {
        UnitUI.Spawn(this, uiHeight, uiSize);
    }

    /// <summary>
    /// Handles all important movement updates for the monster.
    /// </summary>
    private void HandleMovement()
    {
        if (CanMove())
        {
            if (target != null && !InRange(target.transform.position))
            {
                agentNavigation.SetDestination(target.transform.position);
            }
            
            else if (target == null && alliedFaction == Faction.Player &&!InRange(GameManager.player.transform.position))
            { agentNavigation.SetDestination(GameManager.player.transform.position); }
            
            else 
            { StopMoving(); }
        }
        else
        { StopMoving(); }
    }

    private void HandleFleeMovement()
    {
        if (CanMove()) {
            
            // Generate zigzag flee points if non present
            if (zigzagPoints == null || currentPointIndex >= zigzagPoints.Length) {
                // Calculate the escape direction
                Vector3 directionToPlayer = (transform.position - targetPosition).normalized;
                GenerateZigzagPoints(directionToPlayer);
            }

            // Check to see if still running from the player
            float distanceFromPlayerToPoint = Vector3.Distance(zigzagPoints[currentPointIndex], targetPosition);
            float distanceFromThisMonsterToPoint = Vector3.Distance(this.transform.position, zigzagPoints[currentPointIndex]);
            if (distanceFromPlayerToPoint < distanceFromThisMonsterToPoint)
            {
                // If player closer regenerate new escape list 
                Vector3 directionToPlayer = (transform.position - targetPosition).normalized;
                GenerateZigzagPoints(directionToPlayer);
            }

            // Move to the current zigzag point
            if (currentPointIndex < zigzagPoints.Length) {
                agentNavigation.SetDestination(zigzagPoints[currentPointIndex]);

                // If the NPC reaches the current point, move to the next one
                // NOTE: Redirect to next point should be greater then navAgent stoping distance to avoid slowing down & stopping befor chosing next point

                if (Vector3.Distance(this.transform.position, zigzagPoints[currentPointIndex]) < agentNavigation.stoppingDistance + 2.5f)
                    currentPointIndex++;
            }
        }
        else { StopMoving(); }

        // Generates an array of zigzag points based on the escape direction
        void GenerateZigzagPoints(Vector3 escapeDirection) {
            zigzagPoints = new Vector3[zigzagPointsCount];
            Vector3 startPoint = transform.position;

            for (int i = 0; i < zigzagPointsCount; i++) {
                // Calculate the next point along the escape direction
                Vector3 nextPoint = startPoint + escapeDirection * zigzagDistance;

                // Apply random lateral offset for zigzagging
                float offset = Random.Range(-zigzagOffset, zigzagOffset);
                Vector3 lateralOffset = Vector3.Cross(Vector3.up, escapeDirection).normalized * offset;
                nextPoint += lateralOffset;

                // Ensure the point is on the NavMesh
                if (NavMesh.SamplePosition(nextPoint, out NavMeshHit hit, 1f, NavMesh.AllAreas)) {
                    zigzagPoints[i] = hit.position;
                }
                else {
                    zigzagPoints[i] = nextPoint; // Fall back if no NavMesh
                }

                // Move to the next point in the sequence
                startPoint = zigzagPoints[i];
            }

            // Reset the current point index to start the new path
            currentPointIndex = 0;
        }
    }


    /// <summary>
    /// Removes the outline from this unit.
    /// TODO: Move this into the outline class, it has no business being here.
    /// </summary>
    private void RemoveOutline()
    {
        if (outline != null)
        {
            outline.enabled = false;
            outline.OnDisable();
            outline.OutlineWidth = 0.0f;
            outline.OutlineColor = Color.black;
            outline.UpdateMaterialProperties();
        }
    }

    /// <summary>
    /// Handles the visual outline updates for this unit.
    /// TODO: Fix this if possible - there's presumably a performance hit here.
    /// </summary>
    private void UpdateOutline()
    {
        if (outline == null) return;

        outline.OutlineColor = GameManager.hoveredMonster == this ? color : colorFade;
        outline.UpdateMaterialProperties();
        outline.enabled = false;
        outline.enabled = true;
    }

    /// <summary>
    /// Attempts to cast all abilities on the unit (if all requirements are met).
    /// </summary>
    protected override void TryToCastAbilities()
    {
        base.TryToCastAbilities();
        foreach (var ability in abilities)
        {
            if (ability != null)
            {
                CastAbility(ability, target, targetPosition);
            }
        }
    }

    /// <summary>
    /// Makes the unit take the given amount of damage, displaying a damage number
    /// above the unit.
    /// </summary>
    public override void TakeDamage(float amount, bool isCritical, Unit damagingUnit, IEngineHandler damageSource)
    {
        base.TakeDamage(amount, isCritical, damagingUnit, damageSource);
        if (GameManager.settings.showDamageNumbers)
        {
            amount = ApplyDamageFormula(amount, isCritical, damagingUnit, damageSource);
            Vector3 spawnPos = transform.position + Vector3.up + Random.insideUnitSphere * DamageNumberSpawnVariance;
            Color color = isCritical ? CritDamageTextColor : HitDamageTextColor;
            float scale = isCritical ? CriticalDamageNumberScaleMod : 1.0f;
            StatusMessageUI.Spawn(spawnPos, Mathf.Max(0, Mathf.Round(amount)).ToString(), color, scale);
        }
    }

    /// <summary>
    /// Performs all kill cleanup actions (removing outlines, components, effects, auras, etc.).
    /// </summary>
    private void HandleKillCleanup()
    {
        RemoveOutline();
        if (hasAnimations)
        {
            animator.transform.SetParent(null);
            Destroy(animator.gameObject, corpseDuration);
        }

        foreach (var destroyAt in GetComponentsInChildren<DestroyAt>())
        {
            destroyAt.transform.SetParent(null);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Kills this unit, spawning gold, items, and giving experience to the player.
    /// </summary>
    public override void Kill(Unit killingUnit, IEngineHandler killingSource, bool isCritical)
    {
        if (isDead) return;

        base.Kill(killingUnit, killingSource, isCritical);
        HandleKillCleanup();

        HandleMonsterDrops();
        GiveExperienceToPlayer();
    }

    /// <summary>
    /// Handles monster drops upon death.
    /// </summary>
    private void HandleMonsterDrops()
    {
        float random = Random.value;

        if (isEmpowered)
        {
            HandleEmpoweredDrops(random);
        }
        else
        {
            HandleUnempoweredDrops(random);
        }
    }

    /// <summary>
    /// Handles drops for empowered monsters.
    /// </summary>
    virtual protected void HandleEmpoweredDrops(float random)
    {
        if (random < GameManager.empoweredMonsterValues.empoweredMonsterLegendaryDropChance)
        {
            ItemPickup.Spawn(transform.position, Item.ItemRarity.Legendary);
        }
        else if (random < GameManager.empoweredMonsterValues.empoweredMonsterRareDropChance)
        {
            ItemPickup.Spawn(transform.position, Item.ItemRarity.Rare);
        }
        else if (random < GameManager.empoweredMonsterValues.empoweredMonsterCommonDropChance)
        {
            ItemPickup.Spawn(transform.position, Item.ItemRarity.Common);
        }

        HealthPickup.Spawn(transform.position);
    }

    /// <summary>
    /// Handles drops for unempowered monsters.
    /// </summary>
    virtual protected void HandleUnempoweredDrops(float random)
    {
        if (random < GameManager.monsterValues.unempoweredMonsterLegendaryDropChance)
        {
            ItemPickup.Spawn(transform.position, Item.ItemRarity.Legendary);
        }
        else if (random < GameManager.monsterValues.unempoweredMonsterRareDropChance)
        {
            ItemPickup.Spawn(transform.position, Item.ItemRarity.Rare);
        }
        else if (random < GameManager.monsterValues.unempoweredMonsterCommonDropChance)
        {
            ItemPickup.Spawn(transform.position, Item.ItemRarity.Common);
        }

        if (Random.value < GameManager.monsterValues.goldDropChance)
        {
            GoldPickup.Spawn(transform.position, Mathf.RoundToInt(Random.Range(GameManager.monsterValues.baseGoldDropAmountMinimum,
                GameManager.monsterValues.baseGoldDropAmountMaximum) * goldModifier));
        }

        if (Random.value < (GameManager.healthGlobeValues.baseHealthGlobeChance - 
            GameManager.healthGlobeValues.reducedChancePerExistingGlobe * HealthPickup.activeHealthGlobes))
        {
            HealthPickup.Spawn(transform.position);
        }
    }

    /// <summary>
    /// Gives experience to the player after killing the monster.
    /// </summary>
    private void GiveExperienceToPlayer()
    {
        float xp = GameManager.playerExperienceValues.baseMonsterXP * experienceModifier;

        if (isEmpowered)
        {
            xp *= GameManager.empoweredMonsterValues.empoweredMonsterXPModifier;
        }

        GameManager.player.AddExperience(xp);
    }

    /// <summary>
    /// Calculates the ideal range for this monster to keep between itself and
    /// the player.
    /// </summary>
    private void CalculateTargetRange()
    {
        float stoppingDistance = MaximumPossibleRange;

        foreach (var ability in abilities)
        {
            if (ability.AbilityRequiresRange())
            {
                stoppingDistance = Mathf.Min(stoppingDistance, ability.range - StopDistanceBuffer);
            }
            else if (ability.AbilityRequiresMelee())
            {
                stoppingDistance = baseAttackRange - StopDistanceBuffer;
            }
        }

        agentNavigation.stoppingDistance = stoppingDistance;
    }

    /// <summary>
    /// Chooses the target unit and location for this monster (e.g., for ability targeting).
    /// </summary>
    private void CalculateMonsterTargeting()
    {
        target = Utilities.GetClosestEnemy(transform.position, 10f, this);
        if (target != null)
        {
            targetPosition = target.transform.position;
        }
    }

    /// <summary>
    /// Empowers this monster, increasing its size, stats, and rewards.
    /// </summary>
    public void Empower()
    {
        if (isEmpowered) return;

        isEmpowered = true;
        baseMaxHealth *= GameManager.empoweredMonsterValues.empoweredMonsterHealthModifier;
        baseDamage *= GameManager.empoweredMonsterValues.empoweredMonsterDamageModifier;
        baseAttacksPerSecond *= GameManager.empoweredMonsterValues.empoweredMonsterAttackSpeedModifier;
        transform.localScale *= EmpoweredScaleModifier;
        uiSize *= EmpoweredUISizeModifier;
        uiHeight *= EmpoweredUIHeightModifier;
        healthBarType = HealthBarType.Empowered;

        GameObject obj = Instantiate(GameManager.assets.empoweredEffect, transform);
        obj.transform.position += Vector3.up;
    }


    
}