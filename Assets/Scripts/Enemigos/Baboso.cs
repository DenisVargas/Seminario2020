using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using IA.StateMachine.Generic;
using IA.LineOfSight;
using Core.DamageSystem;

public enum DamageType
{
    e_fire,
    cutting,
    blunt,
    piercing
}

public struct Damage
{
    public DamageType type;
    public float Ammount;
    public float criticalMultiplier;
    public bool instaKill;
}

[System.Serializable]
public struct DamageModifier
{
    public DamageType type;
    public float percentual;
}

public class Baboso : MonoBehaviour, IDamageable<Damage>, IAgressor<Damage, HitResult>
{
    [Header("Stats")]
    [SerializeField] float _health = 10;
    [SerializeField] float _attackRange = 5;

    [SerializeField] DamageModifier[] weaknesses;  //Aumentan el daño multiplicandolo x un porcentaje.
    [SerializeField] DamageModifier[] resistances; //reducen el daño x el un porcentaje.

    [Header("Aditional Options")]
    [SerializeField] bool startPatrolling = false;
    [SerializeField] float _burnTime = 3f;

    float _remainingBurnTime = 0f;

    [Header("References")]
    [SerializeField]
    Vector3 targetPosition                 = Vector3.zero;
    [SerializeField] Waypoint patrolPoints = null;
    //[SerializeField] int _toStopPositions  = 0;
    [SerializeField] float stopTime        = 1.5f;

    BabosoState _currentState;
    BabosoState _previousState;
    BabosoState _nextState;

    #region StateMachine
    public enum BabosoState
    {
        idle,
        patroll,
        pursue,
        attack,
        think,
        burning,
        dead
    }
    GenericFSM<BabosoState> state = null;
    #endregion

    #region Animaciones
    Animator _anims;
    int[] _animHash = new int[3];
    bool _a_Walk
    {
        get => _anims.GetBool(_animHash[0]);
        set => _anims.SetBool(_animHash[0], value);
    }
    bool _a_Dead
    {
        get => _anims.GetBool(_animHash[1]);
        set => _anims.SetBool(_animHash[1], value);
    }
    bool _a_burning
    {
        get => _anims.GetBool(_animHash[2]);
        set => _anims.SetBool(_animHash[2], value);
    } 
    bool _a_attack
    {
        get => _anims.GetBool(_animHash[3]);
        set => _anims.SetBool(_animHash[3], value);
    }
    #endregion

    NavMeshAgent _agent;
    LineOfSightComponent _sight;
    Trail _trail;

    HurtBox _hurtbox;
    HitBox _hitbox;
    Damage _damageState = new Damage();
    Rigidbody _rb = null;

    Transform _target = null;
    private Vector3   _targetLocation;
    private bool      _stoping;
    private int _PositionsMoved;
    private float _remainingStopTime;

#if UNITY_EDITOR
    [Space,Header("Debug Options")]
    [SerializeField] Color attackRangeColor = Color.white;
#endif

    float health
    {
        get => _health;
        set
        {
            _health = value;
            if (_health <= 0)
            {
                _health = 0;
                state.Feed(BabosoState.dead);
            }
        }
    }

    public bool Attack_isInCooldown { get; private set; }

    private void Awake()
    {
        //Seteo todas las referencias a los componentes.
        _agent = GetComponent<NavMeshAgent>();
        _sight = GetComponent<LineOfSightComponent>();
        _trail = GetComponentInChildren<Trail>();
        _hurtbox = GetComponentInChildren<HurtBox>();
        _anims = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _remainingStopTime = stopTime;

        //AutoSet Del Target.
        var tar = FindObjectOfType<NMA_Controller>();
        if (tar != null)
        {
            _target = tar.transform;
            _sight.SetTarget(_target);
        }
        else
        {
            Debug.LogError(string.Format("{0} No encontró un Target Válido\nTe olvidaste de poner o activar al Player en la escena Salame >:/", gameObject.name));
        }

        //Convertimos strings a hash para obtener las animaciones más rápidamente.
        _animHash = new int[4];
        var animparams = _anims.parameters;
        for (int i = 0; i < _animHash.Length; i++)
        {
            _animHash[i] = animparams[i].nameHash;
            //print("Parámetro es: " + animparams[i].name + " y su valor es: " + animparams[i].type);
        }

        //State Machine
        var dead = new State<BabosoState>("Dead");
        var idle = new State<BabosoState>("Idle");
        var patroll = new State<BabosoState>("Patroll");
        var pursue = new State<BabosoState>("Pursue");
        var attack = new State<BabosoState>("Attack");
        var burning = new State<BabosoState>("Burning");
        var think = new State<BabosoState>("Think");

        #region Transiciones
        idle.AddTransition(BabosoState.dead, dead)
            .AddTransition(BabosoState.burning, burning)
            .AddTransition(BabosoState.think, think);

        patroll.AddTransition(BabosoState.pursue, pursue)
               .AddTransition(BabosoState.burning, burning)
               .AddTransition(BabosoState.think, think)
               .AddTransition(BabosoState.dead, dead);

        pursue.AddTransition(BabosoState.think, think)
              .AddTransition(BabosoState.attack, attack)
              .AddTransition(BabosoState.burning, burning)
              .AddTransition(BabosoState.dead, dead);

        attack.AddTransition(BabosoState.dead, dead)
              .AddTransition(BabosoState.burning, burning)
              .AddTransition(BabosoState.think, think);

        burning.AddTransition(BabosoState.think, think)
               .AddTransition(BabosoState.dead, dead);

        think.AddTransition(BabosoState.dead, dead)
             .AddTransition(BabosoState.idle, idle)
             .AddTransition(BabosoState.burning, burning)
             .AddTransition(BabosoState.patroll, patroll)
             .AddTransition(BabosoState.pursue, pursue);

        //Esto es cuando se resetea, si estamos utilizando un pool de enemigos.
        dead.AddTransition(BabosoState.idle, idle)
            .AddTransition(BabosoState.patroll, patroll);
        #endregion

        dead.OnEnter += (x) => 
        {
            _currentState = BabosoState.dead;
            //Seteo animación.
            _a_Dead = true;
            _a_Walk = false;

            if (_agent.isActiveAndEnabled)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.isStopped = false;
            }

            _rb.useGravity = false;
            _rb.velocity = Vector3.zero;

            //Apago componentes que no hagan falta.
            //gameObject.SetActive(false);
            _trail.Emit = false;
        };
        //dead.OnUpdate += () => { };
        dead.OnExit += (x) => { };

        idle.OnEnter += (previousState) => 
        {
            _currentState = BabosoState.idle;
            //Seteo animacion
            _a_Walk = false;
            _a_Dead = false;
            _a_burning = false;

            //print(string.Format("{0}: Entró al estado Idle", gameObject.name));
        };
        idle.OnUpdate += () => { };
        idle.OnExit += (nextState) => 
        {
            print(string.Format("{0}: Salió del estado Idle", gameObject.name));
        };

        patroll.OnEnter += (x) => 
        {
            _currentState = BabosoState.patroll;
            //Seteo la animacion.
            _a_Walk = true;

            //Obtengo un target Point para moverme.
            _targetLocation = patrolPoints.getNextPosition();
            _stoping = false;

            if (!_stoping && _agent.destination != _targetLocation)
            {
                _agent.SetDestination(_targetLocation);
            }
        };
        patroll.OnUpdate += () => 
        {
            if (_agent.remainingDistance < 1)
            {
                //_PositionsMoved++;
                //_stoping = _PositionsMoved >= _toStopPositions;
                _agent.SetDestination(patrolPoints.getNextPosition());
                _stoping = true;
            }

            //Timing.
            if (_stoping)
            {
                //Debug.Log("entre al stopping");
                
                _remainingStopTime -=Time.deltaTime;
                _agent.isStopped = true;
                //Debug.Log(_remainingStopTime);
                if (_remainingStopTime <= 0)
                {
                    //Debug.Log("Entre Al desstoping");
                    // _agent.SetDestination(patrolPoints.getNextPosition());
                    _remainingStopTime = stopTime;
                    _agent.isStopped = false;
                    _stoping = false;
                }
            }

            //Si veo al enemigo paso a perseguirlo.
            if (_sight.IsInSight(_target))
                state.Feed(BabosoState.pursue);
        };
        patroll.OnExit += (x) => 
        {
            _a_Walk = false;
        };

        pursue.OnEnter += (x) => 
        {
            _currentState = BabosoState.pursue;
            _a_Walk = true;
        };
        pursue.OnUpdate += () => 
        {
            _agent.SetDestination(_target.position);

            if (Vector3.Distance(_target.position, transform.position) < _attackRange)
            {
                state.Feed(BabosoState.attack);
            }
        };
        pursue.OnExit += (x) => 
        {
            if (x == BabosoState.idle || x == BabosoState.dead || x == BabosoState.burning )
            {
                _a_Walk = false;
            }
        };

        attack.OnEnter += (previousState) =>
        {
            _currentState = BabosoState.attack;
            _a_attack = true;

            //Instakilleo al target.
            KillTarget();

            //Detengo la marcha.
            _agent.isStopped = true;
            //Debug.LogWarning("Attack On Enter");
        };
        //attack.OnUpdate += () => { };
        attack.OnExit += (nextState) =>
        {
            _a_attack = false;
            //Debug.LogWarning("Attack On Exit");
        };

        burning.OnEnter += (previousState) =>
        {
            _currentState = BabosoState.burning;
            //Prendemos la animación de quemado.
            _a_burning = true;
            _a_Walk = false;

            //En propósitos de debug.
            //var matColor = GetComponentInChildren<MeshRenderer>().material;
            //matColor.color = Color.red;
            _remainingBurnTime = _burnTime;

            //Dejamos de emitir baba.
            _trail.Emit = false;

            //Nos detenemos y reseteamos el camino.
            _agent.isStopped = true;
            _agent.ResetPath();
        };
        burning.OnUpdate += () => { };
        //burning.OnExit += (previousState) =>
        //{
        //    //Cuando se termina el tiempo de la animación saltamos a dead.
        //};

        think.OnEnter += (x) => 
        {
            _currentState = BabosoState.think;
        };
        think.OnUpdate += () => 
        {
            //Tomo desiciones... pero cuales?
            //Si mi enemigo esta muerto.
            state.Feed(BabosoState.patroll);
        };
        think.OnExit += (x) => 
        {

        };

        state = startPatrolling ? new GenericFSM<BabosoState>(patroll) : new GenericFSM<BabosoState>(idle);

#if UNITY_EDITOR
        state.Debug_Transitions = false; 
#endif
    }

    // Update is called once per frame
    void Update()
    {
        //Hago las weas.
        state.Update();
        //print("Current State is:" + _currentState.ToString());
    }


    //========================================== Member Funcs =================================================

    void KillTarget()
    {
        if (_target != null)
        {
            var killeable = _target.GetComponent<IDamageable<Damage>>();
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

    //========================================== Sistema de Daño ==============================================

    //Recibimos Daño
    public void Hit(Damage damage)
    {
        Debug.LogWarning(string.Format("{0} ha recibido un HIT", gameObject.name));

        //Si me alcanza el daño de fuego.
        if (damage.instaKill && damage.type == DamageType.e_fire)
        {
            if (_currentState != BabosoState.burning)
                state.Feed(BabosoState.burning);
            return;
        }

        if (damage.instaKill)
        {
            if (_currentState != BabosoState.dead)
                health = 0;
            return;
        }

        health -= damage.Ammount;
    }
    //Devolvemos nuestras estadísticas.
    public Damage getDamageState()
    {
        return _damageState;
    }
    //En este caso si resivo un hit, pos me muero asi que no pasa mucho.
    public void HitStatus(HitResult result)
    {
        //Cuando conecta un hit... no hago nada en particular.
    }

    //===================================== Animation Events ===================================================

    //Fases de Ataques --> StartUp, Active, Recovery
    //Por ahora el juego es Instakill, asi que cuando un enemigo te alcanza, te golpea y tu mueres.

    public void AV_Attack_End()
    {
        state.Feed(BabosoState.think);

        //Debug.LogWarning("AttackEnded");
    }

    public void AV_Burning_End()
    {
        state.Feed(BabosoState.dead);
        Debug.LogWarning("AnimEvent: BurningEnd");
    }

    //===================================== DEBUG ===============================================================

    private void OnDrawGizmosSelected()
    {
        //Rango de ataque
        Gizmos.color = attackRangeColor;
        Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}
