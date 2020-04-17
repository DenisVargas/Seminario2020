using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.StateMachine.Generic;
using UnityEngine.AI;
using IA.LineOfSight;

public class Baboso : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] float _health = 10;
    [SerializeField] float _attackRange;

    [Header("Aditional Options")]
    [SerializeField] bool startPatrolling = false;

    [Header("References")]
    [SerializeField]
    Vector3 targetPosition = Vector3.zero;
    [SerializeField] Waypoint patrolPoints;
    [SerializeField] int _toStopPositions;
    [SerializeField] float stopTime;

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
        dead
    }
    GenericFSM<BabosoState> state = null; 
    #endregion

    NavMeshAgent _agent;
    Animator     _anims;
    LineOfSightComponent _sight;

    [SerializeField] Transform _target;
    private Vector3   _targetLocation;
    private bool      _stoping;
    private int _PositionsMoved;
    private float _remainingStopTime;

#if UNITY_EDITOR
    [Space,Header("Debug Options")]
    [SerializeField] Color attackRangeColor;
#endif

    public float health
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

        //State Machine
        var dead = new State<BabosoState>("Dead");
        var idle = new State<BabosoState>("Idle");
        var patroll = new State<BabosoState>("Patroll");
        var pursue = new State<BabosoState>("Pursue");
        var attack = new State<BabosoState>("Attack");
        var think = new State<BabosoState>("Think");

        #region Transiciones
        idle.AddTransition(BabosoState.dead, dead)
            .AddTransition(BabosoState.think, think);

        patroll.AddTransition(BabosoState.pursue, pursue)
               .AddTransition(BabosoState.think, think)
               .AddTransition(BabosoState.dead, dead);

        pursue.AddTransition(BabosoState.think, think)
              .AddTransition(BabosoState.attack, attack)
              .AddTransition(BabosoState.dead, dead);

        attack.AddTransition(BabosoState.dead, dead)
              .AddTransition(BabosoState.think, think);

        think.AddTransition(BabosoState.dead, dead)
             .AddTransition(BabosoState.idle, idle)
             .AddTransition(BabosoState.patroll, patroll)
             .AddTransition(BabosoState.pursue, pursue);
        #endregion

        dead.OnEnter += (x) => 
        {
            //Seteo animación.
            //Apago componentes que no hagan falta.
            gameObject.SetActive(false);
        };
        //dead.OnUpdate += () => { };
        dead.OnExit += (x) => { };

        idle.OnEnter += (x) => 
        {
            //Seteo animacion
            print("Estoy quieto");
        };
        idle.OnUpdate += () => { };
        idle.OnExit += (x) => { };

        patroll.OnEnter += (x) => 
        {
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

        pursue.OnEnter += (x) => { };
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

        think.OnEnter += (x) => { };
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
}
