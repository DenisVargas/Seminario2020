using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using IA.FSM;
using IA.LineOfSight;
using Core.DamageSystem;
using Core.Debuging;

[RequireComponent(typeof(Animator))]
public class Grunt : MonoBehaviour, IDamageable<Damage, HitResult>, IInteractable, ILivingEntity
{
    public Action<GameObject> OnEntityDead = delegate { };
    public void SubscribeToLifeCicleDependency(Action<GameObject> OnEntityDead)
    {
        this.OnEntityDead += OnEntityDead;
    }
    public void UnsuscribeToLifeCicleDependency(Action<GameObject> OnEntityDead)
    {
        this.OnEntityDead += OnEntityDead;
    }

    [Header("Stats")]
    [SerializeField] float _health = 100f;
    [SerializeField] float _maxHealth = 100f;
    [SerializeField] float _attackRange = 2f;
    [SerializeField] float _minDetectionRange = 3f;
    

    [Header("Interaction System")]
    [SerializeField] float _safeInteractionDistance = 5f;
    [SerializeField] List<OperationType> _suportedOperations = new List<OperationType>();

    public float Health
    {
        get => _health;
        set
        {
            _health = value;
            if (_health <= 0)
            {
                _health = 0;
                //state.Feed(BoboState.dead);
            }
            Hook_HealthUpdate(_health);
        }
    }
    public Vector3 position => transform.position;
    public Vector3 LookToDirection => transform.forward;

    //------------------------------------ HOOKS --------------------------------------------

    Action<float> Hook_HealthUpdate = delegate { }; //Un Hook por cada estadística que requiere una UI.

    //---------------------------------------------------------------------------------------

    [Space()]
    [SerializeField] DamageModifier[] Weaknesses; //Aumentan el daño ingresante.
    [SerializeField] DamageModifier[] resistances;//reducen el daño ingereante.

    //-------------------------------- State Machine -----------------------------------------

    FiniteStateMachine<CommonState> _states = new FiniteStateMachine<CommonState>();
    private Damage _currentDamageState = new Damage() { Ammount = 10 };

    //----------------------------------- Targeting -----------------------------------------

    IDamageable<Damage, HitResult> AttackTarget = null;
    Controller _player;
    ClonBehaviour _playerClone;
    bool _playerFound = false;

    //----------------------------------- Animation -----------------------------------------

    private Animator _anim = null;
    int[] _animHash = new int[6];
    bool _a_walk
    {
        get => _anim.GetBool(_animHash[0]);
        set => _anim.SetBool(_animHash[0], value);
    }
    bool _a_Burning
    {
        get => _anim.GetBool(_animHash[1]);
        set => _anim.SetBool(_animHash[1], value);
    }
    bool _a_GetHit
    {
        get => _anim.GetBool(_animHash[2]);
        set => _anim.SetBool(_animHash[2], value);
    }
    bool _a_Attack
    {
        get => _anim.GetBool(_animHash[3]);
        set => _anim.SetBool(_animHash[3], value);
    }
    bool _a_SmashDown
    {
        get => _anim.GetBool(_animHash[4]);
        set => _anim.SetBool(_animHash[4], value);
    }
    bool _a_Dead
    {
        get => _anim.GetBool(_animHash[5]);
        set => _anim.SetBool(_animHash[5], value);
    }
    bool _a_targetFinded
    {
        get => _anim.GetBool(_animHash[6]);
        set => _anim.SetBool(_animHash[6], value);
    }

    public bool IsAlive { get; private set; } = (true);
    public bool IsCurrentlyInteractable { get; private set; } = (true);
    public int InteractionsAmmount => _suportedOperations.Count;

    //----------------------------------- Components ----------------------------------------

    private Rigidbody _rb = null;
    private Collider _mainCollider = null;
    LineOfSightComponent _sight = null;

    #region DEBUG
#if UNITY_EDITOR
    [Space(), Header("DEBUG GIZMOS")]
    [SerializeField] bool DEBUG_MINDETECTIONRANGE = true;
    [SerializeField] Color DEBUG_MINDETECTIONRANGE_COLOR = Color.cyan;
    [SerializeField] bool DEBUG_INTERACTION_RAIDUS = true;

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        if (DEBUG_INTERACTION_RAIDUS)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _safeInteractionDistance);
        }
        if (DEBUG_MINDETECTIONRANGE)
        {
            Gizmos.color = DEBUG_MINDETECTIONRANGE_COLOR;
            Gizmos.DrawWireSphere(transform.position, _minDetectionRange);
        }
    }
#endif 
    #endregion

    //=======================================================================================

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _mainCollider = GetComponent<Collider>();

        _health = _maxHealth;

        var player = FindObjectOfType<Controller>();
        if (player != null)
        {
            _player = player;

            ClonBehaviour clon = player.Clon;
            if (clon != null)
                _playerClone = clon;
            else
                Debug.LogError("La cagaste, el clon no esta seteado.");
        }

        #region Declaración y Set de Estados

        IdleState idle = GetComponent<IdleState>();
        idle.checkForPlayerAndClone = checkForPlayerOrClone;
        idle.AttachTo(_states, true);

        //var think = new State<BoboState>("Think");
        //var wander = new State<BoboState>("Wander");
        BurningState burning = GetComponent<BurningState>();
        burning.AttachTo(_states);

        FallTrapState falling = GetComponent<FallTrapState>();
        falling.AttachTo(_states);

        RageState rage = GetComponent<RageState>();
        rage.AttachTo(_states);

        PursueState pursue = GetComponent<PursueState>();
        pursue.AttachTo(_states);

        AttackState attack = GetComponent<AttackState>();
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

        //wander.AddTransition(dead)
        //    .AddTransition(think)
        //    .AddTransition(falling)
        //    .AddTransition(idle);

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

    // Update is called once per frame
    void Update()
    {
        _states.Update();
    }

    //=================================== Private Memeber Funcs =============================

    void ResetSetUp()
    {
        _health = _maxHealth;
        IsAlive = true;
    }
    void MoveToTarget(Vector3 targetPosition)
    {
        transform.forward = (Vector3.Normalize((targetPosition - transform.position).YComponent(0)));

        //if (targetPosition != _agent.destination)
        //    _agent.SetDestination(targetPosition);
    }

    void LookAtAttackTarget() { }
    void KillAttackTarget()
    {
        if (AttackTarget != null && AttackTarget.IsAlive)
        {
            FeedDamageResult(AttackTarget.GetHit(new Damage() { instaKill = true, type = DamageType.piercing, KillAnimationType = 1 }));
        }
    }
    void checkForPlayerOrClone()
    {
        if (_player != null && _playerClone != null)
        {
            float distToPlayer = (_player.transform.position - transform.position).magnitude;
            float distToClone = (_playerClone.transform.position - transform.position).magnitude;
            float mindist = float.MaxValue;
            IDamageable<Damage, HitResult> closerTarget = null;

            if (_player.IsAlive)
            {
                if (_sight.IsInSight(_player.transform) && distToPlayer < mindist
                    || distToPlayer < _minDetectionRange)
                        closerTarget = _player;
            }

            if (_playerClone.IsAlive)
            {
                if (_sight.IsInSight(_playerClone.transform) && distToClone < mindist ||
                    distToClone < _minDetectionRange)
                        closerTarget = _playerClone;
            }

            if (closerTarget != null)
            {
                var currentTarget = closerTarget;
                if (currentTarget != null)
                    AttackTarget = currentTarget;
                else
                    Debug.LogError("La cagaste, el objetivo no es Damageable");
            }
        }
    }

    //============================== State Machine Acces ====================================

    public void ChangeStateTo(CommonState input)
    {
        _states.Feed(input);
    }

    //==================================== Damage System ====================================

    public HitResult GetHit(Damage damage)
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
    public Damage GetDamageStats()
    {
        //Retornamos nuestras estadísticas de combate actuales.
        return _currentDamageState;
    }
    public void FeedDamageResult(HitResult result)
    {
        //Si cause daño efectivamente.
        if (result.conected && result.fatalDamage)
        {
            Core.Debuging.Console.instance.Print($"{gameObject.name} ha conectado un golpe directo y ha matado a su objetivo", DebugLevel.info);
            _a_targetFinded = false;
            AttackTarget = null;
        }
    }
    public void GetStun(Vector3 AgressorPosition, int PosibleKillingMethod){}

    //================================ Interaction System ===================================

    public InteractionParameters GetSuportedInteractionParameters()
    {
        return new InteractionParameters()
        {
            LimitedDisplay = false,
            SuportedOperations = _suportedOperations
        };
    }

    public void OnOperate(OperationType selectedOperation, params object[] optionalParams)
    {
        switch (selectedOperation)
        {
            case OperationType.Ignite:
                OnIgnite(optionalParams);
                break;
            case OperationType.TrowRock:
                OnHitWithRock(optionalParams);
                break;
            default:
                break;
        }
    }
    private void OnIgnite(object[] optionalParams)
    {
        //Si recibe daño igniteante, este puede prenderse fuego.
        //state.Feed(BoboState.burning);
    }
    private void OnHitWithRock(object[] optionalParams)
    {
        //Si es atacado por una roca, entra en rageMode.
        print("Me han pegado con una roca, hijos de puta!");
        //state.Feed(BoboState.rage);
    }
    public Vector3 requestSafeInteractionPosition(IInteractor requester)
    {
        return transform.position + ((requester.position - transform.position).normalized * _safeInteractionDistance);
    }
    public void OnConfirmInput(OperationType selectedOperation, params object[] optionalParams)
    {
        //Acá podemos bloquear su comportamiento quizás.
    }
    public void OnCancelOperation(OperationType operation, params object[] optionalParams)
    {
        //Esto se llama cuando se cancela la operación.
    }

    //============================== Animation Events =======================================

    void AV_AttackStart()
    {
    }
    void AV_Attack_Hit()
    {
        KillAttackTarget();
        _a_Attack = false;
    }
    void AV_Attack_Ended()
    {
        var _currentState = _states.currentState.StateType;

        if (_currentState == CommonState.rage)
        {
            //state.Feed(BoboState.idle);
        }

        if (_currentState == CommonState.attack)
        {
            checkForPlayerOrClone();
            if (AttackTarget == null || !AttackTarget.IsAlive)
            {
                //state.Feed(BoboState.idle);
            }
            else
            {
                //float distanceToTarget = Vector3.Distance(transform.position, _killeableTarget.transform.position);
                //state.Feed(distanceToTarget < _attackRange ? BoboState.attack : BoboState.pursue);
            }
        }
    }
    void AV_HitReact_End()
    {
        _a_GetHit = false;
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
