using System;
using System.Collections;
using UnityEngine;
using Core.DamageSystem;
using IA.FSM;

public class Baboso : BaseNPC
{
    [Header("Aditional Options")]
    [SerializeField] float _desapearEffectDelay = 4f;
    [SerializeField] float _timeToDesapear      = 4f;
    [SerializeField] float _burnTime            = 3f;
    private float _remainingBurnTime            = 0f;
    [SerializeField] bool startPatrolling       = false;

    [Header("References")]
    [SerializeField] GameObject[] burnParticles   = new GameObject[2];

    Collider _mainCollider      = null;
    HurtBox _hurtbox            = null;
    HitBox _hitbox              = null;
    Damage _damageState         = new Damage();
    Trail _trail                = null;

    #region DEBUG
    #if UNITY_EDITOR
    [Space, Header("Debug Options")]
    [SerializeField] bool DEBUG_ATACKRANGE = false;
    [SerializeField] Color DEBUG_AttackRangeColor = Color.white;
    [SerializeField] bool DEBUG_MINDETECTIONRANGE = false;
    [SerializeField] Color DEBUG_MINDETECTIONRANGE_COLOR = Color.white;

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));

        if (DEBUG_ATACKRANGE)
        {
            Gizmos.color = DEBUG_AttackRangeColor;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
        if (DEBUG_MINDETECTIONRANGE)
        {
            Gizmos.color = DEBUG_MINDETECTIONRANGE_COLOR;
            Gizmos.DrawWireSphere(transform.position, _minDetectionRange);
        }
    }

    #endif 
    #endregion

    private float Health
    {
        get => _health;
        set
        {
            _health = value;
            if (_health <= 0)
                _health = 0;
        }
    }

    public bool Attack_isInCooldown { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        //Seteo todas las referencias a los componentes.
        _mainCollider = GetComponent<Collider>();
        _hurtbox = GetComponentInChildren<HurtBox>();
        _trail = GetComponentInChildren<Trail>();

        //State Machine

        IdleState idle = GetComponent<IdleState>();
        idle.checkForPlayerAndClone = checkForPlayerOrClone;
        idle.AttachTo(_states);

        PatrollState patroll = GetComponent<PatrollState>();
        patroll.checkForPlayer = checkForPlayerOrClone;
        patroll.moveToNode = MoveToNode;
        patroll.AttachTo(_states);

        if (startPatrolling)
        {
            _states.SetState(CommonState.patroll);
            _trail.EnableEmission();
        }
        else _states.SetState(CommonState.idle);

        DeadState dead = GetComponent<DeadState>();
        dead.OnDead = OnEntityDead;
        dead.Reset = ResetSetUp;
        dead.AttachTo(_states);

        PursueState pursue = GetComponent<PursueState>();
        pursue.checkDistanceToTarget = TargetIsInAttackRange;
        pursue.getDestinyNode = getCloserNodeToAttackTarget;
        pursue.MoveToTarget = MoveToNode;
        pursue.AttachTo(_states);

        AttackState attack = GetComponent<AttackState>();
        attack.LookAtAttackTarget = LookAtAttackTarget;
        attack.StunAttackTarget = StunTarget;
        attack.KillAttackTarget = KillAttackTarget;
        attack.Think = () => { };
        attack.AttachTo(_states);

        FallTrapState falling = GetComponent<FallTrapState>();
        falling.AttachTo(_states);

        BurningState burning = GetComponent<BurningState>();
        burning.OnBurning = () =>
        {
            StartCoroutine(Burn());

            //Dejamos de emitir baba.
            _trail.DisableTrailEmission();
        };
        burning.AttachTo(_states);

        ExplodeState explode = GetComponent<ExplodeState>();
        explode.DeactivateComponents = () => 
        {
            _rb.useGravity = false;
            _rb.velocity = Vector3.zero;
            _mainCollider.enabled = false;
            _trail.DisableTrailEmission();
        };
        explode.AttachTo(_states);

        #region Transiciones
        idle.AddTransition(dead)
            .AddTransition(pursue)
            .AddTransition(burning)
            .AddTransition(explode)
            .AddTransition(falling);

        patroll.AddTransition(pursue)
               .AddTransition(falling)
               .AddTransition(burning)
               .AddTransition(explode)
               .AddTransition(dead);

        explode.AddTransition(dead);

        pursue.AddTransition(attack)
              .AddTransition(falling)
              .AddTransition(burning)
              .AddTransition(explode)
              .AddTransition(dead);

        falling.AddTransition(dead);

        attack.AddTransition(dead)
              .AddTransition(attack)
              .AddTransition(burning)
              .AddTransition(falling)
              .AddTransition(explode)
              .AddTransition(patroll)
              .AddTransition(idle);

        burning.AddTransition(falling)
               .AddTransition(explode)
               .AddTransition(dead);

        dead.AddTransition(falling)
            .AddTransition(idle)
            .AddTransition(explode)
            .AddTransition(patroll);

        #endregion
    }
    protected override void Update()
    {
        base.Update();
    }

    //========================================== Sistema de Daño ==============================================

    public override HitResult GetHit(Damage damage)
    {

        HitResult result = new HitResult()
        {
            conected = true,
            fatalDamage = true
        };

        CommonState _currentState = _states.CurrentStateType;
        if (_currentState == CommonState.dead) return result;

        //Debug.LogWarning(string.Format("{0} ha recibido un HIT", gameObject.name));
        if (damage.instaKill)
        {
            if (damage.type == DamageType.e_fire && _currentState != CommonState.burning)
            {
                _states.Feed(CommonState.burning);
            }
            if (damage.type == DamageType.blunt && _currentState != CommonState.crushed)
            {
                _states.Feed(CommonState.crushed);
            }
            if (damage.type == DamageType.piercing && _currentState != CommonState.dead)
            {
                _states.Feed(CommonState.dead);
            }
        }
        else
        {
            Health -= damage.Ammount;
        }
        return result;
    }
    public override void FeedDamageResult(HitResult result){ }
    public override Damage GetDamageStats()
    {
        return _damageState;
    }
    public override void GetStun(Vector3 AgressorPosition, int PosibleKillingMethodID) { }

//===================================== Animation Events ===================================================

    //Fases de Ataques --> StartUp, Active, Recovery
    //Por ahora el juego es Instakill, asi que cuando un enemigo te alcanza, te golpea y tu mueres.

    void AV_Attack_Start()
    {
        if (_attackTarget != null)
            transform.forward = (_attackTarget.transform.position - transform.position).normalized;
    }
    void AV_Attack_Land()
    {
        if (_attackTarget != null)
            KillAttackTarget();
    }
    void AV_Attack_End()
    {
        if (_attackTarget != null)
        {
            if (_attackTarget.IsAlive)
            {
                if (Vector3.Distance(transform.position, _attackTarget.transform.position) < _attackRange)
                    _states.Feed(CommonState.attack);
                else
                    _states.Feed(CommonState.pursue);
            }
            else
            {
                _attackTarget = null;
                if (startPatrolling)
                    _states.Feed(CommonState.patroll);
                else
                    _states.Feed(CommonState.idle);
            }
        }
    }
    void AV_Burning_End()
    {
        _states.Feed(CommonState.dead);
        Debug.LogWarning("AnimEvent: BurningEnd");
    }

    IEnumerator Burn()
    {
        burnParticles[0].SetActive(true);
        yield return new WaitForSeconds(0.1f);
        burnParticles[1].SetActive(true);
    }
}
