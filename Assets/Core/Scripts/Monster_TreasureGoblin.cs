using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unit;

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
    
}
