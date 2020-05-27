using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using IA.StateMachine.Generic;
using IA.LineOfSight;
using Core.DamageSystem;

public struct HitResult
{
    public bool conected;
    public bool fatalDamage;
}

public struct DamageAcumulation
{
    public DamageType type;
    public float TimeRemaining;
    public float Ammount;
}

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(LineOfSightComponent))]
public class Grunt : MonoBehaviour, IDamageable<Damage>, IAgressor<Damage, HitResult>, IInteractable
{
    [Header("Stats")]
    [SerializeField] float _health = 100f;
    [SerializeField] float _maxHealth = 100f;
    [SerializeField] float _attackRange = 2f;

    [Header("Interaction System")]
    [SerializeField] float _safeInteractionDistance = 5f;
    [SerializeField] List<OperationType> ValidOperations = new List<OperationType>();

    public float Health
    {
        get => _health;
        set
        {
            _health = value;
            if (_health <= 0)
            {
                _health = 0;
                state.Feed(BoboState.dead);
            }
            Hook_HealthUpdate(_health);
        }
    }
    public Vector3 position => transform.position;
    public Vector3 LookToDirection => transform.forward;

    //------------------------------------ HOOKS --------------------------------------------

    //Un Hook por cada estadística que requiere una UI.
    Action<float> Hook_HealthUpdate = delegate { };

    //---------------------------------------------------------------------------------------

    [Space()]
    [SerializeField]DamageModifier[] Weaknesses; //Aumentan el daño ingresante.
    [SerializeField]DamageModifier[] resistances;//reducen el daño ingereante.

    //--------------------------------- Rage Mode -------------------------------------------

    [Header("Rage Mode")]
    [SerializeField] LayerMask _targeteables = ~0;
    [SerializeField] float _rageMode_Duration = 10f;
    [SerializeField] float _rageMode_TargetDetectionRange = 5;

    private bool _rageMode      = false;
    private float _rageModeTime = 0f;

    //--------------------------------- Wander State ----------------------------------------

    [Header("Wander State")]
    [SerializeField] float _wanderMinDistance = 2f;
    [SerializeField] float _wanderMaxDistance = 10f;
    [SerializeField] float _minWanderTime     = 2f;
    [SerializeField] float _maxWanderTime     = 10f;

    private float _wanderingTime = 0f;

    //-------------------------------- State Machine -----------------------------------------

    public enum BoboState
    {
        invalid,
        idle,
        wander,
        pursue,
        think,
        burning,
        fallTrap,
        rage,
        attack,
        dead
    }

    private GenericFSM<BoboState> state = null;
    [SerializeField] BoboState _currentState = BoboState.invalid;
    //[SerializeField] BoboState _chainState   =  BoboState.invalid;

    private Damage _currentDamageState = new Damage() { Ammount = 10 };
    //Dictionary<DamageType, List<DamageAcumulation>> DamageStack = new Dictionary<DamageType, List<DamageAcumulation>>();

    //----------------------------------- Targeting -----------------------------------------

    private Transform _targetTransform;
    private Vector3 _targetPosition;

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
    

    //----------------------------------- Components ----------------------------------------

    private Rigidbody _rb = null;
    private Collider _mainCollider = null;
    private NavMeshAgent _agent = null;
    private LineOfSightComponent _sight = null;

#if UNITY_EDITOR
    //----------------------------------- DEBUG ---------------------------------------------   
    [Space(), Header("DEBUG GIZMOS")]
    [SerializeField] bool DEBUG_WanderStateRanges = true;
    [SerializeField] Color DEBUG_WanderRange_Min_GIZMOCOLOR = Color.blue;
    [SerializeField] Color DEBUG_WanderRange_Max_GIZMOCOLOR = Color.blue;
    [Space]
    [SerializeField] bool DEBUG_RageMode_Target = true; 
    [SerializeField] bool DEBUG_RageMode_Ranges = true;
    [SerializeField] Color DEBUG_RM_TrgDetectRange_GIZMOCOLOR = Color.blue;
    [SerializeField] bool DEBUG_INTERACTION_RAIDUS = true;

#endif

    //=======================================================================================

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _sight = GetComponent<LineOfSightComponent>();
        _rb = GetComponent<Rigidbody>();
        _mainCollider = GetComponent<Collider>();

        _health = _maxHealth;

        var target = FindObjectOfType<NMA_Controller>();
        if (target != null)
        {
            _targetTransform = target.transform;
            _targetPosition = target.transform.position;
            _sight.SetTarget(_targetTransform);
        }

        _anim = GetComponent<Animator>();
        _animHash = new int[6];
        for (int i = 0; i < _animHash.Length; i++)
            _animHash[i] = _anim.parameters[i].nameHash;

        #region Declaración de Estados
        var idle = new State<BoboState>("Idle");
        var wander = new State<BoboState>("Wander");
        var burning = new State<BoboState>("Burning");
        var falling = new State<BoboState>("FallingTrap");
        var rage = new State<BoboState>("Rage");
        var pursue = new State<BoboState>("Pursue");
        var think = new State<BoboState>("Think");
        var attack = new State<BoboState>("Attack");
        var dead = new State<BoboState>("Dead");
        #endregion

        #region Transiciones.
        idle.AddTransition(BoboState.dead, dead)
            .AddTransition(BoboState.pursue, pursue)
            .AddTransition(BoboState.fallTrap, falling)
            .AddTransition(BoboState.think, think);

        wander.AddTransition(BoboState.dead, dead)
            .AddTransition(BoboState.think, think)
            .AddTransition(BoboState.fallTrap, falling)
            .AddTransition(BoboState.idle, idle);

        pursue.AddTransition(BoboState.dead, dead)
              .AddTransition(BoboState.attack, attack)
              .AddTransition(BoboState.fallTrap, falling)
              .AddTransition(BoboState.think, think);

        think.AddTransition(BoboState.dead, dead)
             .AddTransition(BoboState.idle, idle)
             .AddTransition(BoboState.fallTrap, falling)
             .AddTransition(BoboState.wander, wander)
             .AddTransition(BoboState.pursue, pursue)
             .AddTransition(BoboState.attack, attack);

        attack.AddTransition(BoboState.dead, dead)
              .AddTransition(BoboState.think, think)
              .AddTransition(BoboState.fallTrap, falling)
              .AddTransition(BoboState.pursue, pursue);

        falling.AddTransition(BoboState.dead, dead);

        burning.AddTransition(BoboState.dead, dead)
               .AddTransition(BoboState.think, think)
               .AddTransition(BoboState.fallTrap, falling);

        rage.AddTransition(BoboState.dead, dead)
            .AddTransition(BoboState.pursue, pursue)
            .AddTransition(BoboState.fallTrap, falling);

        dead.AddTransition(BoboState.idle, idle);
        #endregion

        #region Eventos de Estado
        idle.OnEnter += (x) =>
        {
            _currentState = BoboState.idle;
            if (state != null)
            {
                if (state.current != null)
                    print(string.Format("{0} ha entrado al estado {1}", gameObject.name, state.current.StateName));
                else
                    print(string.Format("{0} ha iniciado en el estado Idle", gameObject.name));
            }
            //Por defecto nos aseguramos que no este reproduciendo walk.
            _a_walk = false;
        };
        idle.OnUpdate += ()=> 
        {
            //Si veo al enemigo pero no lo estoy persiguiendo, entonces...
            if (_sight.IsInSight(_targetTransform) && _currentState != BoboState.pursue)
            {
                state.Feed(BoboState.pursue);
            }
        };
        //idle.OnExit += (NextState)=> {};

        wander.OnEnter += (x) =>
        {
            print(string.Format("{0} ha entrado al estado {1}", gameObject.name, state.current.StateName));

            //Reproduzco la animación correspondiente.
            _wanderingTime = 0;
        };
        wander.OnUpdate += () =>
        {
            // Reduzco un timer, cuando el tiempo llegue a 0. 
            _wanderingTime -= Time.deltaTime;
            if (_wanderingTime <= 0)
            {
                // Busco un nuevo targetPosition;
                _targetPosition = getRandomPosition();
                _wanderingTime = UnityEngine.Random.Range(_minWanderTime, _maxWanderTime);
            }
        };
        //wander.OnExit += (NextState) => { };

        burning.OnEnter += (x) =>
        {
            _currentState = BoboState.burning;
            //Seteo la animación.
        };
        burning.OnUpdate += () =>
        {
            #region TODO
            //Tengo un timing en el que estoy prendido fuego...
            //Por cada segundo que estoy prendido fuego, me aplico daño x acumulación.
            //List<int> indexesToDelete = new List<int>();

            //float fireDamage = 0;
            //for (int i = 0; i < DamageStack[DamageType.e_fire].Count; i++)
            //{
            //    var acum = DamageStack[DamageType.e_fire][i];
            //    fireDamage += acum.Ammount;

            //    float time = acum.TimeRemaining;
            //    time -= Time.deltaTime;
            //    if (time <= 0)
            //        indexesToDelete.Add(i);
            //}

            //foreach (var index in indexesToDelete)
            //{
            //    DamageStack[DamageType.e_fire].RemoveAt(index);
            //}

            //Health -= fireDamage; 
            #endregion

            //Esto estaría kul, pero por ahora solo me muero al terminar la animación.
            //Espero a que la animación termine.

            //Igual que en attack, reviso todo lo que sea relevante al ataque
            //Si no veo a un player y sigo vivo cuando termino de prenderme fuego.
            //Entro en rage.
        };
        //burning.OnExit += (nextState) => { };

        falling.OnEnter += (x) =>
        {
            _rb.useGravity = true;
            _mainCollider.isTrigger = true;

            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.enabled = false;
        };

        pursue.OnEnter += (x) =>
        {
            _currentState = BoboState.pursue;
            _a_walk = true;
        };
        pursue.OnUpdate += () =>
        {
            //Me muevo en dirección al objetivo.
            if (_targetTransform != null)
                MoveToTarget(_targetTransform.position);

            //Si la distancia de ataque es menor a un treshold.
            float dst = Vector3.Distance(transform.position, _targetTransform.position);
            if (dst <= _attackRange)
            {
                //Ataco.
                state.Feed(BoboState.attack);
            }
        };
        pursue.OnExit += (NextState) =>
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            if (NextState == BoboState.burning)
            {
                _a_walk = false;
            }
        };

        think.OnEnter += (x) =>
        {
            _currentState = BoboState.think;
            //Si tengo un tiempo de reacción
            //Seteo el tiempo de reacción.
            //Sino
            //Tomo una desición en base a los inputs.
            if (_sight.distanceToTarget < _attackRange)
            {
                state.Feed(BoboState.attack);
            }
            else if (_sight.IsInSight(_targetTransform) && _sight.distanceToTarget > _attackRange)
            {
                state.Feed(BoboState.pursue);
            }
        };
        think.OnUpdate += () =>
        {
            //Actualizo el tiempo de reacción restante.
            //Si el tiempo de reacción es 0.
            //Tomo una desición en base a los inputs.
        };
        //think.OnExit += (NextState) => { };

        attack.OnEnter += (x) =>
        {
            _currentState = BoboState.attack;
            _a_Attack = true;

            KillTarget();
        };
        attack.OnUpdate += () =>
        {
            //Chequeo información relevante para el estado del ataque.
            //Cuando el ataque termine, utiliza esta info para determinar que acción realizar consecutivamente.

            //Mini think!!
            //Si estoy en modo Rage, marco el siguiente estado como RageWaitState.
            if (_rageMode)
            {
                //_chainState = BoboState.pursue;
            }

            //Si no estoy en modo Rage y el target esta muerto/destruido.
            //Si todavía estoy en rageMode.
            //Selecciono otro target.
            //Sino estoy fuera de rageMode
            //Vuelvo al estado idle.
            //Opcionalmente puedo hacer un festejo.
        };

        rage.OnEnter += (x) =>
        {
            _currentState = BoboState.rage;
            //Seteo la animación -> Necesito mostrarle al player que me di cuenta que entre en rage!
            _anim.SetBool((int)BoboState.rage, true);

            //Mientras espero a que la animación termine...

            //Si no estoy en rageMode
            if (!_rageMode)
            {
                //Marco el estado a rageMode!
                _rageMode = true;
                //Inicio el contador.
                _rageModeTime = _rageMode_Duration;
            }

            //Selecciono un nuevo Target y marco el siguiente estado como persue.
            SelectRageModeTarget();

            //Cuando termina la animación pasa el estado pursue.
        };

        dead.OnEnter += (x) =>
        {
            _currentState = BoboState.dead;
            _a_Dead = true;
        };
        dead.OnExit += (NextState) =>
        {
            //Si nextState es idle -> Significa que estamos usando este NPC en un Pool y queremos reactivarlo.
            if (NextState == BoboState.idle)
            {
                ResetSetUp();
            }
        }; 
        #endregion

        //State Machine
        state = new GenericFSM<BoboState>(idle);
    }

    // Update is called once per frame
    void Update()
    {
        state.Update();
    }
    
    //=================================== Private Memeber Funcs =============================

    private void ResetSetUp()
    {
        _health = _maxHealth;
    }
    void MoveToTarget(Vector3 targetPosition)
    {
        transform.forward = ( Vector3.Normalize((targetPosition - transform.position).YComponent(0)));

        if (targetPosition != _agent.destination)
            _agent.SetDestination(targetPosition);
    }
    void SelectRageModeTarget()
    {
        //Priorizamos las unidades vivas.
        //Si no hay unidades vidas seleccionamos el target destructible mas cercano.

        //Raycast en área.
        var posibleTargets = Physics.OverlapSphere(transform.position, _rageMode_TargetDetectionRange, _targeteables);

        var closerTarget = posibleTargets
                          .OrderBy(x =>
                          {
                              return Vector3.Distance(transform.position, 
                                                      x.gameObject.transform.position);
                          })
                          .FirstOrDefault();

        if (closerTarget != null)
        {
            //Paso a think.
            state.Feed(BoboState.think);
        }
        else
        {
            _targetTransform = closerTarget.transform;
        }
    }
    void UpdateRageModeState()
    {
        _rageModeTime -= Time.deltaTime;

        if (_rageModeTime < 0)
        {
            //Acá puedo añadir cosas.
            _rageModeTime = 0;
        }
    }
    Vector3 getRandomPosition()
    {
        //Calculo un target Random.
        var RandomTargetPosition = new Vector3(UnityEngine.Random.Range(0, 1),
                                                 0,
                                                 UnityEngine.Random.Range(0, 1));
        float randomDistance = UnityEngine.Random.Range(_wanderMinDistance, _wanderMaxDistance);
        return transform.position + (RandomTargetPosition * randomDistance);
    }
    void KillTarget()
    {
        if (_targetTransform != null)
        {
            var killeable = _targetTransform.GetComponent<IDamageable<Damage>>();
            if (killeable != null)
            {
                killeable.Hit(new Damage() { instaKill = true });
            }
            else
                Debug.LogError("La cagaste, el target no es Damageable");
        }
        else
            Debug.LogError("La cagaste, el target es nulo");
    }

    //============================== State Machine Acces ====================================

    public void ChangeStateTo(BoboState input)
    {
        state.Feed(input);
    }

    //==================================== Damage System ====================================

    public void Hit(Damage damage)
    {
        //Al recibir daño...
        //Si estamos en idle, wander o think
        //Entramos en rage.

        if (damage.instaKill)
        {
            if (_currentState != BoboState.dead)
                Health = 0;
            return;
        }

        Health -= damage.Ammount;
    }

    public Damage getDamageState()
    {
        //Retornamos nuestras estadísticas de combate actuales.
        return _currentDamageState;
    }

    public void HitStatus(HitResult result)
    {
        //Si cause daño efectivamente.
        if (result.conected)
        {
            //El golpe conectó con el objetivo.

            //Si objetivo fue destruido y sigo en ragemode...
            if (result.fatalDamage && _rageMode)
            {
                //Festejo y busco un nuevo target.
                //Seteo la animación de festejo.
                _anim.SetBool(10, true);
                //Actualizo un nuevo target, mientras busco el estado.
                SelectRageModeTarget();
                return;
            }
        }
    }


    //================================ Interaction System ===================================

    public InteractionParameters GetSuportedInteractionParameters()
    {
        return new InteractionParameters()
        {
            LimitedDisplay = false,
            SuportedOperations = ValidOperations
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
        state.Feed(BoboState.burning);
    }
    private void OnHitWithRock(object[] optionalParams)
    {
        //Si es atacado por una roca, entra en rageMode.
        state.Feed(BoboState.rage);
    }
    public Vector3 requestSafeInteractionPosition(IInteractor requester)
    {
        return ((requester.position - transform.position).normalized * _safeInteractionDistance);
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

    public void AV_Attack_Ended()
    {
        state.Feed(BoboState.think);
    }

    //=============================== DEBUG =================================================
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (DEBUG_RageMode_Ranges)
        {
            Gizmos.color = DEBUG_RM_TrgDetectRange_GIZMOCOLOR;
            Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
            Gizmos.DrawWireSphere(transform.position, _rageMode_TargetDetectionRange);
        }

        if (DEBUG_RageMode_Target && _targetTransform != null)
        {
            bool isVisible = _sight.IsInSight(_targetTransform);
            Gizmos.color = isVisible ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, _targetTransform.position);
        }

        if (DEBUG_WanderStateRanges)
        {
            Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));

            Gizmos.color = DEBUG_WanderRange_Min_GIZMOCOLOR;
            Gizmos.DrawWireSphere(transform.position, _wanderMinDistance);

            Gizmos.color = DEBUG_WanderRange_Max_GIZMOCOLOR;
            Gizmos.DrawWireSphere(transform.position, _wanderMaxDistance);
        }

        if (DEBUG_INTERACTION_RAIDUS)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
            Gizmos.DrawWireSphere(transform.position, _safeInteractionDistance);
        }
    }
#endif
}
