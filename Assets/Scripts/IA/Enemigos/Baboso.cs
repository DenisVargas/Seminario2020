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
    [SerializeField] float _attackRange;

    [SerializeField] DamageModifier[] weaknesses;  //Aumentan el daño multiplicandolo x un porcentaje.
    [SerializeField] DamageModifier[] resistances; //reducen el daño x el un porcentaje.

    [Header("Aditional Options")]
    [SerializeField] bool startPatrolling = false;
    [SerializeField] float _burnTime = 3f;

    float _remainingBurnTime = 0f;

    [Header("References")]
    [SerializeField]
    Vector3 targetPosition = Vector3.zero;
    [SerializeField] Waypoint patrolPoints;
    [SerializeField] int _toStopPositions;
    [SerializeField] float stopTime;

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

    NavMeshAgent _agent;
    Animator     _anims;
    LineOfSightComponent _sight;
    Trail _trail;

    DamageDealer _hurtbox;
    HitBox _hitbox;
    Damage _damageState = new Damage();

    [SerializeField] Transform _target;
    private Vector3   _targetLocation;
    private bool      _stoping;
    private int _PositionsMoved;
    private float _remainingStopTime;

#if UNITY_EDITOR
    [Space,Header("Debug Options")]
    [SerializeField] Color attackRangeColor;
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

    private void Awake()
    {
        //Seteo todas las referencias a los componentes.
        _agent = GetComponent<NavMeshAgent>();
        _sight = GetComponent<LineOfSightComponent>();
        _trail = GetComponentInChildren<Trail>();
        _hurtbox = GetComponentInChildren<DamageDealer>();

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
            //Apago componentes que no hagan falta.
            gameObject.SetActive(false);
        };
        //dead.OnUpdate += () => { };
        dead.OnExit += (x) => { };

        idle.OnEnter += (previousState) => 
        {
            _currentState = BabosoState.idle;
            //Seteo animacion
            print(string.Format("{0}: Entró al estado Idle", gameObject.name));
        };
        idle.OnUpdate += () => { };
        idle.OnExit += (nextState) => 
        {
            print(string.Format("{0}: Salió del estado Idle", gameObject.name));
        };

        patroll.OnEnter += (x) => 
        {
            _currentState = BabosoState.patroll;

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
                _stoping = false;
            }

            //Timing.
            if (_stoping)
            {
                _remainingStopTime--;

                if (_remainingStopTime <= 0)
                {
                    _agent.SetDestination(patrolPoints.getNextPosition());
                    _remainingStopTime = stopTime;
                    _stoping = false;
                }
            }

            //Si veo al enemigo paso a perseguirlo.
            if (_sight.IsInSight(_target))
                state.Feed(BabosoState.pursue);
        };
        patroll.OnExit += (x) => { };

        pursue.OnEnter += (x) => { _currentState = BabosoState.pursue; };
        pursue.OnUpdate += () => 
        {
            //Vector3 direction = (_target.position - transform.position).normalized;

            _agent.SetDestination(_target.position);

            if (Vector3.Distance(_target.position, transform.position) < _attackRange)
            {
                state.Feed(BabosoState.attack);
            }
        };
        pursue.OnExit += (x) => {};

        attack.OnEnter += (previousState) =>
        {
            _currentState = BabosoState.attack;
            //Detengo la marcha.
            _agent.isStopped = true;
            Debug.LogWarning("Attack On Enter");
        };
        attack.OnUpdate += () =>
        {

        };
        attack.OnExit += (nextState) =>
        {
            Debug.LogWarning("Attack On Exit");
        };

        burning.OnEnter += (previousState) =>
        {
            _currentState = BabosoState.burning;
            //Prendemos la animación de quemado.

            //En propósitos de debug.
            var matColor = GetComponentInChildren<MeshRenderer>().material;
            matColor.color = Color.red;
            _remainingBurnTime = _burnTime;

            //Dejamos de emitir baba.
            _trail.Emit = false;

            //Nos detenemos y reseteamos el camino.
            _agent.ResetPath();
        };
        burning.OnUpdate += () =>
        {
            //Esto se maneja x animación.
            if (_remainingBurnTime > 0)
                _remainingBurnTime -= Time.deltaTime;
            else
                state.Feed(BabosoState.dead);
        };
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

            //Cuando dejo de atacar?
        };
        think.OnExit += (x) => 
        {

        };

        state = startPatrolling ? new GenericFSM<BabosoState>(patroll) : new GenericFSM<BabosoState>(idle);
    }

    // Update is called once per frame
    void Update()
    {
        //Hago las weas.
        state.Update();
    }

    private void OnDrawGizmosSelected()
    {
        //Rango de ataque
        Gizmos.color = attackRangeColor;
        Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }

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

    public Damage getDamageState()
    {
        return _damageState;
    }

    public void HitStatus(HitResult result)
    {
        //Cuando conecta un hit... no hago nada en particular.
    }
}
