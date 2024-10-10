using UnityEngine;

public class Monster_Boss : Monster
{
    [SerializeField] private float _healthSecondStage_Min = 0.5f;
    [SerializeField] private float _healthSecondStage_Max = 0.25f;
    [SerializeField] private float _fleeFromTargetTriggerDistance = 5f;
    private bool isEngagedWithPlayer = false;
    [SerializeField] private float _fleeFromTargetMaxDistance;
    private bool _isInSecondStage;


    protected override void Start()
    {
        base.Start();

        isEngagedWithPlayer = false;
        engagementState = EngagementState.MoveTowards;
    }

    protected override void Update()
    {
        base.Update();

        Update_CheckIfInSecondStage();

        if (_isInSecondStage) {
            Update_CatAndMouseMode();
        }      
    }

    private void OnDrawGizmos()
    {
        #region Flee Trigger
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, _fleeFromTargetTriggerDistance);
        #endregion
        #region StopRunningDistance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(this.transform.position, _fleeFromTargetMaxDistance);
        #endregion
    }

    private void Update_CheckIfInSecondStage() {
        float pct_healthRemaining = health / baseMaxHealth;

        if (pct_healthRemaining < _healthSecondStage_Min && pct_healthRemaining > _healthSecondStage_Max)
            _isInSecondStage = true;
        else
            _isInSecondStage = false;
    }

    private void Update_CheckIfFleeTriggered()
    {
            if (Vector3.Distance(this.transform.position, targetPosition) < _fleeFromTargetTriggerDistance)
            {
                engagementState = EngagementState.Flee;
            }    
    }
    private void Update_CatAndMouseMode() {
        if (isEngagedWithPlayer) {
            if (Vector3.Distance(this.transform.position, targetPosition) > _fleeFromTargetMaxDistance) {
                engagementState = EngagementState.StandStill;
            }
            else 
            {
                engagementState = EngagementState.Flee;
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
