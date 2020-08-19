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
using IA.PathFinding;

[RequireComponent(typeof(Animator), typeof(LineOfSightComponent), typeof(PathFindSolver))]
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

    //--------------------------------- UI HOOKS --------------------------------------------

    Action<float> Hook_HealthUpdate = delegate { }; //Un Hook por cada estadística que requiere una UI.

    //---------------------------------------------------------------------------------------

    [Space()]
    [SerializeField] DamageModifier[] Weaknesses; //Aumentan el daño ingresante.
    [SerializeField] DamageModifier[] resistances;//reducen el daño ingereante.

    //-------------------------------- State Machine -----------------------------------------

    FiniteStateMachine<CommonState> _states = new FiniteStateMachine<CommonState>();
    private Damage _currentDamageState = new Damage() { Ammount = 10 };

    //----------------------------------- Targeting -----------------------------------------

    public Vector3 movementTargetPosition => throw new NotImplementedException();
    IDamageable<Damage, HitResult> AttackTarget = null;
    Controller _player;
    ClonBehaviour _playerClone;
    bool _playerFound = false;

    //----------------------------------- Animation -----------------------------------------

    private Animator _anim = null;

    public bool IsAlive { get; private set; } = (true);
    public bool IsCurrentlyInteractable { get; private set; } = (true);
    public int InteractionsAmmount => _suportedOperations.Count;


    //----------------------------------- Components ----------------------------------------

    private Rigidbody _rb = null;
    private Collider _mainCollider = null;
    LineOfSightComponent _sight = null;
    PathFindSolver _solver = null;

    #region DEBUG
#if UNITY_EDITOR
    [Space(), Header("DEBUG GIZMOS")]
    [SerializeField] bool DEBUG_MINDETECTIONRANGE = true;
    [SerializeField] Color DEBUG_MINDETECTIONRANGE_COLOR = Color.cyan;
    [SerializeField] bool DEBUG_INTERACTION_RAIDUS = true;
    [SerializeField] TMPro.TMP_Text DebugText_View;
    string debugText;

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

    //================================ Unity Engine =========================================

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _mainCollider = GetComponent<Collider>();
        _sight = GetComponent<LineOfSightComponent>();
        _solver = GetComponent<PathFindSolver>();
        _anim = GetComponent<Animator>();

        Node _currentCloserNode = _solver.getCloserNode(transform.position);
        transform.position = _currentCloserNode.transform.position;

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
        pursue.checkDistanceToTarget = TargetIsInAttackRange;
        pursue.getDestinyNode = getCloserNodeToAttackTarget;
        pursue.MoveToTarget = MoveToNode;
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

        #if UNITY_EDITOR
            debugText = "";
            debugText += $"Estado: {_states.getCurrentStateType().ToString()}\n";
            debugText += $"Jugador encontrado: {_player != null}\n";
            debugText += $"Clon encontrado: {_playerClone != null}\n";
            DebugText_View.text = debugText; 
        #endif
    }

    //=================================== Private Memeber Funcs =============================

    /// <summary>
    /// Resetea el estado de la vida de esta entidad a su estado originial.
    /// </summary>
    void ResetSetUp()
    {
        _health = _maxHealth;
        IsAlive = true;
    }
    /// <summary>
    /// Mueve a la unidad hasta un nodo determinado con la velocidad específicada.
    /// </summary>
    /// <param name="targetNode">Nodo objetivo al que se quiere moverse.</param>
    /// <param name="movementSpeed">Velocidad del movimiento</param>
    /// <returns>Retorna false si no alcanzó al objetivo, de lo contrario, retorna verdadero si alcanzó al objetivo dentro del treshold correspondiente.</returns>
    bool MoveToNode(Node targetNode, float movementSpeed)
    {
        Vector3 targetDir = (targetNode.transform.position - transform.position).normalized;
        transform.forward = (targetDir.YComponent(0)); //Orientación
        transform.position += targetDir * movementSpeed * Time.deltaTime; //Cambio de posición.

        //Chequeo la distancia al objetivo actual
        return Vector3.Distance(targetNode.transform.position, transform.position) < _solver.ProximityTreshold;
    }
    /// <summary>
    /// Cambia la orientación del NPC para que mire en dirección de su objetivo de ataque.
    /// </summary>
    void LookAtAttackTarget()
    {
        if (AttackTarget != null && AttackTarget.IsAlive)
            transform.forward = (AttackTarget.transform.position - transform.position).normalized;
    }
    void StunTarget()
    {
        if (AttackTarget != null && AttackTarget.IsAlive)
            AttackTarget.GetStun(transform.position, 1);
    }
    void KillAttackTarget()
    {
        if (AttackTarget != null && AttackTarget.IsAlive)
        {
            FeedDamageResult(AttackTarget.GetHit(new Damage() { instaKill = true, type = DamageType.piercing, KillAnimationType = 1 }));
        }
    }
    /// <summary>
    /// Evalúa un cambio de estado en base a la situación actual.
    /// </summary>
    void EvaluateSituation()
    {
        var _currentState = _states.currentState.StateType;

        //Actualizo el estado del player/Clone.
        if (checkForPlayerOrClone()) 
        {
            float distanceToTarget = Vector3.Distance(transform.position, AttackTarget.transform.position);
            _states.Feed(distanceToTarget < _attackRange ? CommonState.attack : CommonState.pursue);
        }
        else
        {
            _states.Feed(CommonState.idle);
        }
    }
    /// <summary>
    /// Chequea si el jugador y su clon estan en rango de visión.
    /// Si se cumple la condición, setea el más cercano como target de Ataque.
    /// </summary>
    bool checkForPlayerOrClone()
    {
        IDamageable<Damage, HitResult> closerTarget = null;
        float distToPlayer = float.MaxValue;
        float distToClone  = float.MaxValue;

        if (_player != null && _player.IsAlive)
        {
            distToPlayer = (_player.transform.position - transform.position).magnitude;
            if (_sight.IsInSight(_player.transform) || distToPlayer < _minDetectionRange)
                closerTarget = _player;
        }

        if (_playerClone != null && _playerClone.IsAlive && _playerClone.isActiveAndEnabled)
        {
            distToClone = (_playerClone.transform.position - transform.position).magnitude;

            if (_sight.IsInSight(_playerClone.transform) && distToClone < distToPlayer ||
                    distToClone < _minDetectionRange)
                closerTarget = _playerClone;
        }

        if (closerTarget != null)
        {
            var currentTarget = closerTarget;
            if (currentTarget != null)
            {
                AttackTarget = currentTarget;
                return true;
            }
            else
                Debug.LogError("La cagaste, el objetivo no es Damageable");
        }

        return false;
    }
    /// <summary>
    /// Chequea si el objetivo de ataque esta en rago de ataque.
    /// </summary>
    /// <returns>True si el objetivo esta en rango de ataque. Falso si el target esta muerto o fuera de rango.</returns>
    bool TargetIsInAttackRange()
    {
        if (AttackTarget == null || !AttackTarget.IsAlive) return false;
        //Si la distancia de ataque es menor al rago de ataque.
        float DistanceToTarget = (AttackTarget.transform.position - transform.position).magnitude;
        return DistanceToTarget < _attackRange;
    }
    /// <summary>
    /// Retorna el nodo mas cercano al objetivo actual de Ataque.
    /// </summary>
    /// <returns>Null si no existe objetivo o si ya esta muerto.</returns>
    Node getCloserNodeToAttackTarget()
    {
        if ((AttackTarget != null && AttackTarget.IsAlive))
        {
            //Calculamos el nodo al que nos queremos mover.
            Node TargetNode = _solver.getCloserNode(AttackTarget.transform.position);
            return TargetNode;
        }
        return null;
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
            _anim.SetBool("TargetFinded", false);
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

    void AV_HitReact_End()
    {
        _anim.SetBool("GetHited", false);
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
