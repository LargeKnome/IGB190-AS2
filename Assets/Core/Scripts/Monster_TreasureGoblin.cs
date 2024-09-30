using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster_TreasureGoblin : Monster
{
    private bool isEngagedWithPlayer = false;
    private bool isGoldSpawning_TimerTrigger = false;
    private bool isGoldSpawning_Movement = false;

    //protected override void Start()
    //{
    //    base.Start();
    //}

    protected override void Update()
    {
        base.Update();
        Update_SpawnGoldTrail();
    }


    private void Update_SpawnGoldTrail()
    {   
        // Only Work if moving (Stops a random pile of gold building up)
        if(agentNavigation.velocity.magnitude > 0.1f)
            isGoldSpawning_Movement = true;
        else 
            isGoldSpawning_Movement = false;

        if (isGoldSpawning_Movement && !isGoldSpawning_TimerTrigger)
            StartCoroutine(SpawnObjectWithDelay());
    }
    IEnumerator SpawnObjectWithDelay()
    {
        isGoldSpawning_TimerTrigger = true; 
        yield return new WaitForSeconds(2f);
        GoldPickup.Spawn(transform.position, Mathf.RoundToInt(Random.Range(1f, 10f) * goldModifier));
        isGoldSpawning_TimerTrigger = false;
    }

    
}
