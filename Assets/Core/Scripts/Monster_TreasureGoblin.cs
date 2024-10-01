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
    [Header("On Death Item Spawn Chance")]
    [Tooltip("Use Values between(0 & 1), larger values increases spawn chance)")]
    [SerializeField] private float _itemSpawnThresholdReduction = 0.5f; //
    [Header("On Death Gold Spawn Chance")]
    private float _onDeathGoldRange = 2.5f;

    protected override void Start()
    {
        base.Start();

        isEngagedWithPlayer = false;

        if (_goldSpawning_ValueMAX < 1)
            _goldSpawning_ValueMAX = 1;

        engagementState = EngagementState.Basic;
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
        random -= _itemSpawnThresholdReduction;
        //NOTE: Override Base
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
        SpawnGoldOnDeath(12); 
    }


    protected override void HandleUnempoweredDrops(float random)
    {
        random -= _itemSpawnThresholdReduction;
        //NOTE: Override Base
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
        SpawnGoldOnDeath(6);
    }
    void SpawnGoldOnDeath(int numberOfPiles)
    {
        float delayBetween = 0.075f;
        for (int i = 0; i < numberOfPiles; i++)
        {
            SpawnGoldAroundUnit();
            //StartCoroutine(SpawnGoldOnDeath(i * delayBetween));
        }
    }

    // NOTE: ISSUE: I dont think this is working because the game object is being destroyed and its endeding all the related coroutines? 
    //IEnumerator SpawnGoldAroundUnit(float delay)
    //{
    //    float spawnRangeMultiplier = 2.5f;

    //    float randX;
    //    float randZ;
    //    Vector3 spawnPoint;

    //    // Random Spawn Point
    //    randX = Random.Range(-1f, 1f);
    //    randZ = Random.Range(-1f, 1f);

    //    spawnPoint = transform.position
    //        + Vector3.forward * randX * spawnRangeMultiplier
    //        + Vector3.left * randZ * spawnRangeMultiplier;

    //    // Spawn the item, after delay         
    //    yield return new WaitForSeconds(delay);
    //    GoldPickup.Spawn(spawnPoint, Mathf.RoundToInt(Random.Range(1f, _goldSpawning_ValueMAX) * goldModifier));       
    //}

    void SpawnGoldAroundUnit()
    {
        // Random Spawn Point
        float randX = Random.Range(-1f, 1f);
        float randZ = Random.Range(-1f, 1f);

        Vector3 spawnPoint = transform.position
            + Vector3.forward * randX * _onDeathGoldRange
            + Vector3.left * randZ * _onDeathGoldRange;

        GoldPickup.Spawn(spawnPoint, Mathf.RoundToInt(Random.Range(1f, _goldSpawning_ValueMAX) * goldModifier));
    }


    #endregion
}
