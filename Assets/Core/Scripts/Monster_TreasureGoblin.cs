using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Monster_TreasureGoblin : Monster
{   
    [Header(" - Treasure Goblin Variables - ")]
    [SerializeField] private float _FleeFromTargetTriggerDistance = 5f;
    private bool isEngagedWithPlayer = false;
    [Header("Gold Trail Variables")]
    [SerializeField] private float _goldSpawning_ValueMAX = 10f;
    [SerializeField] private float _goldSpawning_RateScecond = 2f;
    private bool _isGoldSpawning_TimerTrigger = false;
    private bool _isGoldSpawning_Movement = false;
    [Header("On Death Gold Spawn Chance")]
    private float _onDeathGoldRange = 2.5f;
    [Header("On Death Item Spawn Chance")]
    [SerializeField] private float _itemDropChance_Legendary = 0.15f;
    [SerializeField] private float _itemDropChance_Rare = 0.50f;
    [SerializeField] private float _empoweredDropChanceMultiplier = 2f;


    protected override void Start()
    {
        base.Start();

        isEngagedWithPlayer = false;

        if (_goldSpawning_ValueMAX < 1)
            _goldSpawning_ValueMAX = 1;

        engagementState = EngagementState.MoveTowards;
    }

    protected override void Update()
    {
        base.Update();
        Update_SpawnGoldTrail();
        Update_CheckIfFleeTriggered();
    }
    private void OnDrawGizmos()
    {
        #region Flee Trigger
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, _FleeFromTargetTriggerDistance);
        #endregion

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

    private void Update_SpawnGoldTrail()
    {   
        // Only Work if moving (Stops a random pile of gold building up)
        if(agentNavigation.velocity.magnitude > 0.1f)
            _isGoldSpawning_Movement = true;
        else 
            _isGoldSpawning_Movement = false;

        if (_isGoldSpawning_Movement && !_isGoldSpawning_TimerTrigger)
            StartCoroutine(SpawnObjectWithDelay());
    }
    IEnumerator SpawnObjectWithDelay()
    {
        _isGoldSpawning_TimerTrigger = true; 
        yield return new WaitForSeconds(_goldSpawning_RateScecond);
        GoldPickup.Spawn(transform.position, Mathf.RoundToInt(Random.Range(1f, _goldSpawning_ValueMAX) * goldModifier));
        _isGoldSpawning_TimerTrigger = false;
    }

    private void Update_CheckIfFleeTriggered()
    {
        if (!isEngagedWithPlayer)
        {
            if(Vector3.Distance(this.transform.position,targetPosition) < _FleeFromTargetTriggerDistance)
            {
                // Trigger Flee Mode

                engagementState = EngagementState.Flee;
                isEngagedWithPlayer = true;
            }
        }
    }

    #region Drops
    protected override void HandleEmpoweredDrops(float random)
    {
        //NOTE: Override Base
        if (random < _itemDropChance_Legendary * _empoweredDropChanceMultiplier)
        {
            ItemPickup.Spawn(transform.position, Item.ItemRarity.Legendary);
        }
        else if (random < _itemDropChance_Rare * _empoweredDropChanceMultiplier)
        {
            ItemPickup.Spawn(transform.position, Item.ItemRarity.Rare);
        }
        else 
        {
            ItemPickup.Spawn(transform.position, Item.ItemRarity.Common);
        }
        SpawnGoldOnDeath(12); 
    }


    protected override void HandleUnempoweredDrops(float random)
    {
        //NOTE: Override Base
        if (random < _itemDropChance_Legendary)
        {
            ItemPickup.Spawn(transform.position, Item.ItemRarity.Legendary);
        }
        else if (random < _itemDropChance_Rare)
        {
            ItemPickup.Spawn(transform.position, Item.ItemRarity.Rare);
        }
        else
        {
            ItemPickup.Spawn(transform.position, Item.ItemRarity.Common);
        }
        SpawnGoldOnDeath(6);
    }
    void SpawnGoldOnDeath(int numberOfPiles)
    {
        for (int i = 0; i < numberOfPiles; i++)
        {
            SpawnGoldAroundUnit();
        }
    }
    void SpawnGoldAroundUnit()
    {
        // Random Spawn Point
        float randX = Random.Range(-1f, 1f);
        float randZ = Random.Range(-1f, 1f);

        Vector3 spawnPoint = transform.position
            + Vector3.forward * randX * _onDeathGoldRange
            + Vector3.left * randZ * _onDeathGoldRange;

        //GoldPickup.Spawn(spawnPoint, Mathf.RoundToInt(Random.Range(1f, _goldSpawning_ValueMAX) * goldModifier));
        GoldPickup.Spawn(spawnPoint, Mathf.RoundToInt(Random.Range(GameManager.monsterValues.baseGoldDropAmountMinimum,
                GameManager.monsterValues.baseGoldDropAmountMaximum) * goldModifier));
    }


    #endregion
}
