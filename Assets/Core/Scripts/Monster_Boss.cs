using UnityEngine;

public class Monster_Boss : Monster
{
    [Header("Boss Varaibles")]
    [SerializeField] private float _healthSecondStage_Min = 0.4f;
    [SerializeField] private float _healthSecondStage_Max = 0.2f;

    [SerializeField] private float _fleeFromTargetTriggerDistance = 3f;
    [SerializeField] private float _fleeFromTargetMaxDistance = 6F;

    [SerializeField] private bool _isInSecondStage;

    private bool isPhaseActive;

    private void OnUnitDamaged(GameEvents.OnUnitDamagedInfo damageInfo) 
    {
        if (damageInfo.damagedUnit == this && health < stats.GetValue(Stat.MaxHealth) * 0.5f) 
        {
            isPhaseActive = true;
        }
        else if (damageInfo.damagedUnit == this && health < stats.GetValue(Stat.MaxHealth) * 0.25f) 
        {
            isPhaseActive = false;
        }
    }
    
    protected override void Start()
    {
        base.Start();
        engagementState = EngagementState.MoveTowards;

        GameManager.events.OnUnitDamaged.AddListener(OnUnitDamaged);
    }

    protected override void Update()
    {
        base.Update();

        //Update_CheckIfInSecondStage();

        if (InPhase2() && !InPhase3()) {
            _isInSecondStage = true;
            print("In Seconmd Stage!");
        }
        else if(!InPhase2() || InPhase3()) {
            _isInSecondStage = false;
            print("In 1st or 3 Stage!");         
        }
            
        if (_isInSecondStage) {
            Update_CatAndMouseMode();
        }
    }

    public bool InPhase2() {
        return stats.HasBuffWithLabel("Phase2");
    }
    public bool InPhase3() {
        return stats.HasBuffWithLabel("Phase3");
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
