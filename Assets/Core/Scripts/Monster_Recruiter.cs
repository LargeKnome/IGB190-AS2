using UnityEngine;

public class Monster_Recruiter : Monster
{


    [SerializeField] private float _FleeFromTargetTriggerDistance = 5f;
    private bool isEngagedWithPlayer = false;

    protected override void Start()
    {
        base.Start();

        isEngagedWithPlayer = false;
        engagementState = EngagementState.MoveTowards;
    }

    protected override void Update()
    {
        base.Update();
        Update_CheckIfFleeTriggered();
    }
    private void OnDrawGizmos()
    {
        #region Flee Trigger
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, _FleeFromTargetTriggerDistance);
        #endregion
    }

    private void Update_CheckIfFleeTriggered()
    {
        if (!isEngagedWithPlayer)
        {
            if (Vector3.Distance(this.transform.position, targetPosition) < _FleeFromTargetTriggerDistance)
            {
                // Trigger Flee Mode
                engagementState = EngagementState.Flee;
                isEngagedWithPlayer = true;
            }
        }



    }

    /// <summary>
    /// Attempts to cast all abilities on the unit (if all requirements are met).
    /// </summary>
    protected override void TryToCastAbilities()
    {
        base.TryToCastAbilities();
        //foreach (var ability in abilities)
        //{
        //    if (ability != null)
        //    {
        //        CastAbility(ability, target, targetPosition);
        //    }
        //}
    }


}
