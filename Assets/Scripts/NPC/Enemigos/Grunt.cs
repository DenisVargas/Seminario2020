using System;
using UnityEngine;
using IA.FSM;
using Core.Debuging;
using IA.PathFinding;
using Core.SaveSystem;
using System.Collections.Generic;

public class Grunt : BaseNPC
{
    [Space, Header("==================== GRUNT ========================")]
    [SerializeField] GameObject _trailPrefab    = null;
    [SerializeField] Trail      _trail          = null;
    public float Health
    {
        get => _health;
        set
        {
            _health = value;
            if (_health <= 0)
                _health = 0;
        }
    }

    public Dictionary<CommonState, object> states = new Dictionary<CommonState, object>();

    #region DEBUG
#if UNITY_EDITOR
    [Space(), Header("-------------------------- GIZMOS --------------------------")]
    [SerializeField] string DEBUG_CurrentState = "None/Error";
    [SerializeField] bool DEBUG_MINDETECTIONRANGE = true;
    [SerializeField] Color DEBUG_MINDETECTIONRANGE_COLOR = Color.cyan;
    [SerializeField] TMPro.TMP_Text DebugText_View = null;
    string debugText;

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        if (DEBUG_MINDETECTIONRANGE)
        {
            Gizmos.color = DEBUG_MINDETECTIONRANGE_COLOR;
            Gizmos.DrawWireSphere(transform.position, _minDetectionRange);
        }
    }
    #endif 
    #endregion

    //================================ Unity Engine =========================================

    protected override void Awake()
    {
        base.Awake();
        _mainCollider = GetComponent<Collider>();

        if (_states == null)
            _states = new FiniteStateMachine<CommonState>();

        #region Declaración y Set de Estados

        IdleState idle = GetComponent<IdleState>();
        idle.checkForPlayerAndClone = checkForPlayerOrClone;
        idle.AttachTo(_states, true);
        states.Add(CommonState.idle, idle);

        BurningState burning = GetComponent<BurningState>();
        burning.AttachTo(_states);
        states.Add(CommonState.burning, burning);

        FallTrapState falling = GetComponent<FallTrapState>();
        falling.AttachTo(_states);
        states.Add(CommonState.fallTrap, falling);

        RageState rage = GetComponent<RageState>();
        rage.Set(this, _sight).AttachTo(_states);
        rage.SetAttackTarget = target => _attackTarget = target;
        rage.LookAtTarget = LookAtAttackTarget;
        states.Add(CommonState.rage, rage);

        PursueState pursue = GetComponent<PursueState>().Set(_player, _playerClone);
        pursue.checkDistanceToTarget = TargetIsInAttackRange;
        pursue.getDestinyNode = getCloserNodeToAttackTarget;
        pursue.MoveToTarget = MoveToNode;
        pursue.getTarget = getAttackTarget;
        pursue.AttachTo(_states);
        pursue.TargetIsActiveAndAlive = () =>
        {
            if (_attackTarget != null)
                return _attackTarget.IsAlive && _attackTarget.gameObject.activeSelf;

            return false;
        };
        states.Add(CommonState.pursue, pursue);

        AttackState attack = GetComponent<AttackState>();
        attack.LookAtAttackTarget = LookAtAttackTarget;
        attack.StunAttackTarget = StunTarget;
        attack.KillAttackTarget = KillAttackTarget;
        attack.Think = EvaluateSituation;
        attack.AttachTo(_states);
        states.Add(CommonState.attack, attack);

        DeadState dead = GetComponent<DeadState>();
        dead.OnDead = OnEntityDead;
        dead.Reset = ResetSetUp;
        dead.AttachTo(_states);
        states.Add(CommonState.dead, dead);

        #endregion
        #region Set de Transiciones.
        idle.AddTransition(dead)
            .AddTransition(rage)
            .AddTransition(pursue)
            .AddTransition(falling);

        pursue.AddTransition(dead)
              .AddTransition(attack)
              .AddTransition(falling);

        attack.AddTransition(dead)
              .AddTransition(attack)
              .AddTransition(idle)
              .AddTransition(falling)
              .AddTransition(pursue);

        falling.AddTransition(dead);

        burning.AddTransition(dead)
               .AddTransition(falling);

        rage.AddTransition(dead)
            .AddTransition(idle)
            .AddTransition(pursue)
            .AddTransition(falling);

        dead.AddTransition(idle);
        #endregion
    }
    protected override void Update()
    {
        base.Update(); //_states.Update();

        #if UNITY_EDITOR
            debugText = "";
            debugText += $"Estado: {_states.CurrentStateType.ToString()}\n";
            debugText += $"Jugador encontrado: {_player != null}\n";
            debugText += $"Clon encontrado: {_playerClone != null}\n";
            DebugText_View.text = debugText;

            DEBUG_CurrentState = $"Estado: {_states.CurrentStateType.ToString()}";
        #endif
    }

    //========================================== Sistema de Guardado ==========================================

    public override EnemyData getEnemyData()
    {
        var myData = new EnemyData();
        myData.enemyType = EnemyType.Grunt;
        myData.position = transform.position;
        myData.forward = transform.forward;
        myData.WaypointIDs = new int[0];
        return myData;
    }
    public override void LoadEnemyData(EnemyData enemyData)
    {
        //base.LoadEnemyData(enemyData);
        transform.position = enemyData.position;
        transform.forward = enemyData.forward;
        _states.SetState(CommonState.idle);
    }

    //=================================== Public Memeber Funcs ==============================

    public void AddTrail()
    {
        if(_trail == null)
        {
            var tgo = Instantiate(_trailPrefab, gameObject.transform);
            _trail = tgo.GetComponent<Trail>();
        }
        _trail.gameObject.SetActive(true);

        var patroll = states[CommonState.patroll] as PatrollState;
        patroll.OnUpdateCurrentNode += _trail.OnCloserNodeChanged;
        var pursue = states[CommonState.pursue] as PursueState;
        pursue.OnUpdateCurrentNode += _trail.OnCloserNodeChanged;
    }
    public void RemoveTrail()
    {
        if (_trail == null) return;

        _trail.gameObject.SetActive(false);

        var patroll = states[CommonState.patroll] as PatrollState;
        patroll.OnUpdateCurrentNode -= _trail.OnCloserNodeChanged;
        var pursue = states[CommonState.pursue] as PursueState;
        pursue.OnUpdateCurrentNode -= _trail.OnCloserNodeChanged;
    }

    //=================================== Private Memeber Funcs =============================

    /// <summary>
    /// Evalúa un cambio de estado en base a la situación actual.
    /// </summary>
    void EvaluateSituation()
    {
        var _currentState = _states.currentState.StateType;

        //Actualizo el estado del player/Clone.
        if (checkForPlayerOrClone()) 
        {
            float distanceToTarget = Vector3.Distance(transform.position, _attackTarget.transform.position);
            _states.Feed(distanceToTarget < _attackRange ? CommonState.attack : CommonState.pursue);
        }
        else
        {
            _states.Feed(CommonState.idle);
        }
    }

    //==================================== Damage System ====================================

    public override HitResult GetHit(Damage damage)
    {
        HitResult result = new HitResult() { fatalDamage = true, conected = true };

        if (_states.currentState.StateType == CommonState.dead)
            return new HitResult { conected = false, fatalDamage = false };

        if (damage.type == DamageType.hit)
        {
#if UNITY_EDITOR
            if (debugThisUnit)
                print($"{gameObject.name} Recibió un golpe de tipo {damage.type.ToString()} y es un instakill?: {damage.instaKill}");
#endif

            _states.Feed(CommonState.rage);
        }

        if (damage.instaKill) Health = 0;

        Health -= damage.Ammount;
        if (Health <= 0)
        {
            result.fatalDamage = true;
            result.conected = true;
            _states.Feed(CommonState.dead);
        }

        return result;
    }
    public override Damage GetDamageStats()
    {
        //Retornamos nuestras estadísticas de combate actuales.
        return _defaultDamage;
    }
    public override void FeedDamageResult(HitResult result)
    {
        //Si cause daño efectivamente.
        if (result.conected && result.fatalDamage)
        {
            Core.Debuging.Console.instance.Print($"{gameObject.name} ha conectado un golpe directo y ha matado a su objetivo", DebugLevel.info);
            _anims.SetBool("targetFinded", false);
            _attackTarget = null;
        }
    }
    public override void GetStun(Vector3 AgressorPosition, int PosibleKillingMethod){}

    //============================== Animation Events =======================================

    void AV_HitReact_End()
    {
        print("Hit Reaction Ended");
        _anims.SetBool("GetHited", false);
    }
    void AV_TurnArround_Start()
    {
#if UNITY_EDITOR
        if(debugThisUnit)
            print("Turn Around Started"); 
#endif
        if (_states.CurrentStateType == CommonState.rage)
            (_states.currentState as RageState).SetAnimationStage(1);
    }
    void AV_TurnArround_End()
    {
#if UNITY_EDITOR
        if (debugThisUnit)
            print("Turn Around Ended");
#endif
        if (_states.CurrentStateType == CommonState.rage)
        {
            var rage = _states.currentState as RageState;
            rage.SetAnimationStage(2);
        }
    }
    void AV_Angry_Start()
    {
#if UNITY_EDITOR
        if (debugThisUnit)
            print("Angry Start");
#endif
        if (_states.CurrentStateType == CommonState.rage)
        {
            var rage = _states.currentState as RageState;
            rage.SetAnimationStage(3);
        }
    }
    void AV_Angry_End()
    {
#if UNITY_EDITOR
        if (debugThisUnit)
            print("Angry Ended");
#endif
        if (_states.CurrentStateType == CommonState.rage)
        {
            var rage = _states.currentState as RageState;
            rage.SetAnimationStage(4);
        }
    }
}
