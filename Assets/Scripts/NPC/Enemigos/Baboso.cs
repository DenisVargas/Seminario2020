using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;
using IA.FSM;
using IA.Waypoints;
using Core.SaveSystem;
using System.Linq;

public class Baboso : BaseNPC
{
    [Header("Aditional Options")]
    [SerializeField] bool startPatrolling      = false;
    [SerializeField] CommonState _currentState = CommonState.none;

    [Header("References")]
    [SerializeField] GameObject[] burnParticles   = new GameObject[2];

    Collider _mainCollider      = null;
    HurtBox _hurtbox            = null;
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

        #region State Machine: Estados
        IdleState idle = GetComponent<IdleState>();
        idle.checkForPlayerAndClone = checkForPlayerOrClone;
        idle.AttachTo(_states);

        PatrollState patroll = GetComponent<PatrollState>();
        patroll.checkForPlayer = checkForPlayerOrClone;
        patroll.moveToNode = MoveToNode;
        patroll.OnUpdateCurrentNode = _trail.OnCloserNodeChanged;
        patroll.AttachTo(_states);

        DeadState dead = GetComponent<DeadState>();
        dead.OnDead = OnEntityDead;
        dead.Reset = ResetSetUp;
        dead.AttachTo(_states);

        PursueState pursue = GetComponent<PursueState>();
        pursue.OnUpdateCurrentNode = _trail.OnCloserNodeChanged;
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
            _trail.Emit = false;
        };
        burning.AttachTo(_states);

        ExplodeState explode = GetComponent<ExplodeState>();
        explode.DeactivateComponents = () =>
        {
            _rb.useGravity = false;
            _rb.velocity = Vector3.zero;
            _mainCollider.enabled = false;
            _trail.Emit = false;
        };
        explode.AttachTo(_states); 
        #endregion
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
    private void Start()
    {
        if (startPatrolling)
        {
            _trail.Emit = true; //Es obligatorio que esto pase primero.
            _states.SetState(CommonState.patroll);
        }
        else _states.SetState(CommonState.idle);
    }

    protected override void Update()
    {
        base.Update();
        _currentState = _states.CurrentStateType;
    }

    //========================================== Sistema de Guardado ==========================================

    public override EnemyData getEnemyData()
    {
        var myData = new EnemyData();
        myData.position = transform.position;
        myData.forward = transform.forward;
        myData.enemyType = EnemyType.baboso;
        NodeWaypoint waypoints = GetComponent<NodeWaypoint>();
        myData.WaypointIDs = waypoints.points.Select(x => x.ID).ToArray();

        return myData;
    }

    public override void LoadEnemyData(EnemyData enemyData)
    {
        //Override Completo. Reconstruyo mis waypoints, reposiciono el enemigo y reseteo su estado inicial.
        NodeGraphBuilder graph = FindObjectOfType<NodeGraphBuilder>();
        NodeWaypoint waypoints = GetComponent<NodeWaypoint>();

        waypoints.points = new List<IA.PathFinding.Node>();
        foreach (var ID in enemyData.WaypointIDs)
            waypoints.points.Add(graph.getNode(ID));

        transform.position = enemyData.position;
        transform.forward = enemyData.forward;
        _states.SetState(startPatrolling ? CommonState.patroll : CommonState.idle);
    }

    //========================================== Sistema de Daño ==============================================

    public override HitResult GetHit(Damage damage)
    {
        //Debug.Log("Recibió daño.");
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
            if (damage.type == DamageType.Fire && _currentState != CommonState.burning)
            {
                _states.Feed(CommonState.burning);
            }
            if (damage.type == DamageType.blunt && _currentState != CommonState.explode)
            {
                _states.Feed(CommonState.explode);
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
