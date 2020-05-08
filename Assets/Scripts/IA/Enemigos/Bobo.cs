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


public class Bobo : MonoBehaviour, IDamageable<Damage>, IAgressor<Damage, HitResult>, IInteractable
{
    [Header("Stats")]
    [SerializeField] float _health = 100f;
    [SerializeField] float _maxHealth = 100f;
    [SerializeField] float _attackRange = 2f;

    [SerializeField] List<OperationOptions> Interactions = new List<OperationOptions>();

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

    //------------------------------------ HOOKS --------------------------------------------

    //Un Hook por cada estadística que requiere una UI.
    Action<float> Hook_HealthUpdate = delegate { };

    //---------------------------------------------------------------------------------------

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

    //----------------------------------- Components ----------------------------------------

    private NavMeshAgent _agent = null;
    private LineOfSightComponent _sight = null;
    private Animator _anim = null;


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

#endif

    //=======================================================================================

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _sight = GetComponent<LineOfSightComponent>();
        _sight.SetTarget(FindObjectOfType<NMA_Controller>().gameObject.transform);
        _anim = GetComponent<Animator>();

        _health = _maxHealth;

        _targetTransform = FindObjectOfType<NMA_Controller>().gameObject.transform;

        #region Estados
        var idle = new State<BoboState>("Idle");
        var wander = new State<BoboState>("Wander");
        var burning = new State<BoboState>("Burning");
        var rage = new State<BoboState>("Rage");
        var pursue = new State<BoboState>("Pursue");
        var think = new State<BoboState>("Think");
        var attack = new State<BoboState>("Attack");
        var dead = new State<BoboState>("Dead");
        #endregion

        #region Transiciones.
        idle.AddTransition(BoboState.dead, dead)
            .AddTransition(BoboState.pursue, pursue)
            .AddTransition(BoboState.think, think);

        wander.AddTransition(BoboState.dead, dead)
            .AddTransition(BoboState.think, think)
            .AddTransition(BoboState.idle, idle);

        pursue.AddTransition(BoboState.dead, dead)
              .AddTransition(BoboState.attack, attack)
              .AddTransition(BoboState.think, think);

        think.AddTransition(BoboState.dead, dead)
             .AddTransition(BoboState.idle, idle)
             .AddTransition(BoboState.wander, wander)
             .AddTransition(BoboState.pursue, pursue)
             .AddTransition(BoboState.attack, attack);

        attack.AddTransition(BoboState.dead, dead)
              .AddTransition(BoboState.think, think)
              .AddTransition(BoboState.pursue, pursue);

        burning.AddTransition(BoboState.dead, dead)
               .AddTransition(BoboState.think, think);

        rage.AddTransition(BoboState.dead, dead)
            .AddTransition(BoboState.pursue, pursue);

        dead.AddTransition(BoboState.idle, idle);
        #endregion

        #region Eventos
        idle.OnEnter += (x) =>
        {
            if (state != null)
            {
                if (state.current != null)
                    print(string.Format("{0} ha entrado al estado {1}", gameObject.name, state.current.StateName));
                else
                    print(string.Format("{0} ha iniciado en el estado Idle", gameObject.name));
            }
            //Reproduzco la animacion correspondiente.
        };
        //idle.OnUpdate += ()=> {};
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

        pursue.OnEnter += (x) =>
        {
            //Seteo la animación correspondiente.
            _anim.SetBool("pursuing", true);
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
            //Debería chequear si se cumple el desplazamiento.
        };

        think.OnEnter += (x) =>
        {
            //Si tengo un tiempo de reacción
            //Seteo el tiempo de reacción.
            //Sino
            //Tomo una desición en base a los inputs.
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
            //Seteo la animación.
            //EL ataque se maneja por eventos asi que esto es lo único que hace.
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
        //attack.OnExit += (NextState) => { };

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
        //rage.OnUpdate += () => { };

        dead.OnEnter += (x) =>
        {
            //Seteamos la animación de muerte.
            //Por ahora desactivamos el GameObject.
            gameObject.SetActive(false);
        };
        //dead.OnUpdate += ()=> { };
        dead.OnExit += (NextState) =>
        {
            //Si nextState es idle -> Significa que estamos usando este NPC en un Pool y queremos reactivarlo.
            if (NextState == BoboState.idle)
            {
                ResetSetUp();
                //Reseteamos las estadísticas a las de por defecto.
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

        //Si veo al enemigo pero no lo estoy persiguiendo, entonces...
        if (_sight.IsInSight(_targetTransform) && _currentState != BoboState.pursue)
        {
            state.Feed(BoboState.pursue);
        }
    }
    
    //=================================== Private Memeber Funcs =============================

    private void ResetSetUp()
    {
        _health = _maxHealth;
    }
    void MoveToTarget(Vector3 targetPosition)
    {
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

    //============================== State Machine Acces ====================================

    public void ChangeStateTo(BoboState nextState)
    {
        state.Feed(nextState);
    }

    //==================================== Damage System ====================================

    public void Hit(Damage damage)
    {
        //Al recibir daño...
        //Si estamos en idle, wander o think
            //Entramos en rage.

        //Calculamos el daño.
        //Aplicamos el resultado.
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

    public List<OperationOptions> GetSuportedOperations()
    {
        return Interactions;
    }

    public void Operate(OperationOptions selectedOperation, params object[] optionalParams)
    {
        switch (selectedOperation)
        {
            case OperationOptions.Ignite:
                OnIgnite(optionalParams);
                break;
            case OperationOptions.TrowRock:
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
    } 
#endif
}
