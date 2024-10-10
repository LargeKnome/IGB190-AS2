using UnityEngine;

public class Monster_Boss : Monster
{
    [Header("Boss Varaibles")]
    [SerializeField] private float _healthSecondStage_Min = 0.5f;
    [SerializeField] private float _healthSecondStage_Max = 0.25f;

    [SerializeField] private float _fleeFromTargetTriggerDistance = 3f;
    [SerializeField] private float _fleeFromTargetMaxDistance = 6F;

    [SerializeField] private bool _isInSecondStage;


    protected override void Start()
    {
        base.Start();
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
        {
            _isInSecondStage = false;
            engagementState = EngagementState.MoveTowards;
        }
    }

    private void Update_CatAndMouseMode() {
        float disToPlayer = Vector3.Distance(this.transform.position, targetPosition);
        if (disToPlayer < _fleeFromTargetTriggerDistance) {
            engagementState = EngagementState.Flee;
        }
        else if(disToPlayer > _fleeFromTargetMaxDistance)
        {
            engagementState = EngagementState.StandStill;
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
