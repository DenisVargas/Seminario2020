using System;
using UnityEngine;
using IA.FSM;
using Core.Debuging;
using IA.PathFinding;
using Core.SaveSystem;

public class Grunt : BaseNPC
{
    private Collider _mainCollider = null;

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

    #region DEBUG
    #if UNITY_EDITOR
    [Space(), Header("DEBUG GIZMOS")]
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

        BurningState burning = GetComponent<BurningState>();
        burning.AttachTo(_states);

        FallTrapState falling = GetComponent<FallTrapState>();
        falling.AttachTo(_states);

        RageState rage = GetComponent<RageState>();
        rage.AttachTo(_states);

        PursueState pursue = GetComponent<PursueState>();
        pursue.checkDistanceToTarget = TargetIsInAttackRange;
        pursue.getDestinyNode = getCloserNodeToAttackTarget;
        pursue.MoveToTarget = MoveToNode;
        pursue.getTarget = getAttackTarget;
        pursue.AttachTo(_states);

        AttackState attack = GetComponent<AttackState>();
        attack.LookAtAttackTarget = LookAtAttackTarget;
        attack.StunAttackTarget = StunTarget;
        attack.KillAttackTarget = KillAttackTarget;
        attack.Think = EvaluateSituation;
        attack.AttachTo(_states);

        DeadState dead = GetComponent<DeadState>();
        dead.OnDead = OnEntityDead;
        dead.Reset = ResetSetUp;
        dead.AttachTo(_states);

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
        base.Update();

        #if UNITY_EDITOR
            debugText = "";
            debugText += $"Estado: {_states.CurrentStateType.ToString()}\n";
            debugText += $"Jugador encontrado: {_player != null}\n";
            debugText += $"Clon encontrado: {_playerClone != null}\n";
            DebugText_View.text = debugText; 
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
        print($"Recibió un golpe {damage.type.ToString()} y es un instakill: {damage.instaKill}");
        //Al recibir daño...
        if (_states.currentState.StateType == CommonState.dead)
            return new HitResult { conected = false, fatalDamage = false };


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
        return _currentDamageState;
    }
    public override void FeedDamageResult(HitResult result)
    {
        //Si cause daño efectivamente.
        if (result.conected && result.fatalDamage)
        {
            Core.Debuging.Console.instance.Print($"{gameObject.name} ha conectado un golpe directo y ha matado a su objetivo", DebugLevel.info);
            _anims.SetBool("TargetFinded", false);
            _attackTarget = null;
        }
    }
    public override void GetStun(Vector3 AgressorPosition, int PosibleKillingMethod){}

    //============================== Animation Events =======================================

    void AV_HitReact_End()
    {
        _anims.SetBool("GetHited", false);
    }
    void AV_TurnArround_Start()
    {
        //Core.Debuging.Console.instance.Print($"{gameObject.name}::Evento De Animacion::TurnArround_Start", DebugLevel.info);
        //print($"{gameObject.name}::Evento De Animacion::TurnArround_Start");
        //_hitSecuence = 2;
    }
    void AV_TurnArround_End()
    {
        //_hitSecuence = 3;
        //if (_playerFound || _otherKilleableTargetFounded || _otherDestructibleFounded)
        //    _a_targetFinded = true;
    }
    void AV_Angry_Start()
    {

    }
    void AV_Angry_End()
    {
        //if (_otherKilleableTargetFounded || _otherDestructibleFounded)
        //{
        //    Core.Debuging.Console.instance.Print($"{gameObject.name} ha encontrado a un target válido.", DebugLevel.info);
        //    _killeableTarget = _targetsfounded[0].GetComponent<IDamageable<Damage, HitResult>>();
        //    _a_walk = true;
        //    //state.Feed(BoboState.pursue);
        //}
        //else
        //{
        //    Core.Debuging.Console.instance.Print($"{gameObject.name} no ha encontrado a un target válido.", DebugLevel.error);
        //    //state.Feed(BoboState.idle);
        //}
    }
}
