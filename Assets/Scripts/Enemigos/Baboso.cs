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

public class Baboso : MonoBehaviour, IDamageable<Damage, HitResult>
{
    [Header("Stats")]
    [SerializeField] float _health = 10;
    [SerializeField] float _attackRange = 5;
    [SerializeField] float _explodeRange = 5;

    [SerializeField] DamageModifier[] weaknesses;  //Aumentan el daño multiplicandolo x un porcentaje.
    [SerializeField] DamageModifier[] resistances; //reducen el daño x el un porcentaje.

    [Header("Aditional Options")]
    [SerializeField] bool startPatrolling = false;
    [SerializeField] float _burnTime = 3f;

    float _remainingBurnTime = 0f;

    [Header("References")]
    [SerializeField]
    Vector3 targetPosition = Vector3.zero;
    [SerializeField] Waypoint patrolPoints = null;
    [SerializeField] float stopTime = 1.5f;
    [SerializeField] GameObject[] burnParticles = new GameObject[2];
    [SerializeField] GameObject ExplotionParticle = null;
    [SerializeField] LayerMask _StaineableMask = ~0;

    [SerializeField] BabosoState _currentState;

    #region StateMachine
    public enum BabosoState
    {
        idle,
        patroll,
        pursue,
        attack,
        //think,
        falligTrap,
        burning,
        explode,
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

    Collider _mainCollider = null;
    NavMeshAgent _agent = null;
    LineOfSightComponent _sight;
    Trail _trail;

    HurtBox _hurtbox;
    HitBox _hitbox;
    Damage _damageState = new Damage();
    Rigidbody _rb = null;

    Transform _player = null;
    Transform _playerClone = null;
    Transform _currentTarget = null;
    private Vector3 _targetLocation;
    private bool _stoping;
    private int _PositionsMoved;
    private float _remainingStopTime;

#if UNITY_EDITOR
    [Space, Header("Debug Options")]
    [SerializeField] Color DEBUG_AttackRangeColor = Color.white;
    [SerializeField] Color DEBUG_ExplodeRangeColor = Color.white;
#endif

    float health
    {
        get => _health;
        set
        {
            _health = value;
            if (_health <= 0 && _currentState != BabosoState.dead)
            {
                _health = 0;
                state.Feed(BabosoState.dead);
            }
        }
    }

    public bool Attack_isInCooldown { get; private set; }

    public bool IsAlive { get; private set; } = (true);

    private void Awake()
    {
        //Seteo todas las referencias a los componentes.
        _mainCollider = GetComponent<Collider>();
        _agent = GetComponent<NavMeshAgent>();
        _sight = GetComponent<LineOfSightComponent>();
        _hurtbox = GetComponentInChildren<HurtBox>();
        _anims = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _remainingStopTime = stopTime;
        _trail = GetComponentInChildren<Trail>();

        //AutoSet Del Target.
        var tar = FindObjectOfType<NMA_Controller>();
        if (tar != null)
        {
            _player = tar.transform;
            _playerClone = tar.Clon.transform;
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
        var explode = new State<BabosoState>("Exploded");
        var idle = new State<BabosoState>("Idle");
        var patroll = new State<BabosoState>("Patroll");
        var pursue = new State<BabosoState>("Pursue");
        var attack = new State<BabosoState>("Attack");
        var burning = new State<BabosoState>("Burning");
        var falling = new State<BabosoState>("Falling");
        //var think = new State<BabosoState>("Think");

        #region Transiciones
        idle.AddTransition(BabosoState.dead, dead)
            .AddTransition(BabosoState.pursue, pursue)
            .AddTransition(BabosoState.explode, explode)
            .AddTransition(BabosoState.burning, burning)
            .AddTransition(BabosoState.falligTrap, falling);
            //.AddTransition(BabosoState.think, think);

        patroll.AddTransition(BabosoState.pursue, pursue)
               .AddTransition(BabosoState.falligTrap, falling)
               .AddTransition(BabosoState.burning, burning)
               .AddTransition(BabosoState.explode, explode)
               //.AddTransition(BabosoState.think, think)
               .AddTransition(BabosoState.dead, dead);

        pursue.AddTransition(BabosoState.attack, attack)
              //.AddTransition(BabosoState.think, think)
              .AddTransition(BabosoState.falligTrap, falling)
              .AddTransition(BabosoState.burning, burning)
              .AddTransition(BabosoState.explode, explode)
              .AddTransition(BabosoState.dead, dead);

        falling.AddTransition(BabosoState.dead, dead);

        attack.AddTransition(BabosoState.dead, dead)
              .AddTransition(BabosoState.attack, attack)
              .AddTransition(BabosoState.burning, burning)
              .AddTransition(BabosoState.explode, explode)
              .AddTransition(BabosoState.falligTrap, falling)
              .AddTransition(BabosoState.idle, idle)
              .AddTransition(BabosoState.patroll, patroll);

        burning.AddTransition(BabosoState.falligTrap, falling)
               //.AddTransition(BabosoState.think, think)
               .AddTransition(BabosoState.explode, explode)
               .AddTransition(BabosoState.dead, dead);

        //think.AddTransition(BabosoState.dead, dead)
        //     .AddTransition(BabosoState.idle, idle)
        //     .AddTransition(BabosoState.falligTrap, falling)
        //     .AddTransition(BabosoState.burning, burning)
        //     .AddTransition(BabosoState.explode, explode)
        //     .AddTransition(BabosoState.patroll, patroll)
        //     .AddTransition(BabosoState.pursue, pursue);

        //Esto es cuando se resetea, si estamos utilizando un pool de enemigos.
        dead.AddTransition(BabosoState.idle, idle)
            .AddTransition(BabosoState.explode, explode)
            .AddTransition(BabosoState.patroll, patroll);

        explode.AddTransition(BabosoState.idle, idle);
        #endregion

        dead.OnEnter += (x) => 
        {
            _currentState = BabosoState.dead;
            //Seteo animación.
            _a_Dead = true;
            _a_Walk = false;
            IsAlive = false;

            if (_agent.isActiveAndEnabled)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.isStopped = false;
            }

            _rb.useGravity = false;
            _mainCollider.enabled = false;
            _rb.velocity = Vector3.zero;
            _trail.DisableTrailEmission();

            //Apago componentes que no hagan falta.
            //gameObject.SetActive(false);
        };
        //dead.OnUpdate += () => { };
        dead.OnExit += (x) => 
        {
            IsAlive = true;
        };

        explode.OnEnter += (x) =>
        {
            _currentState = BabosoState.explode;
            //Activo la particula de explosión.
            ExplotionParticle.SetActive(true);
            //Me dejo de mover.
            if (_agent.isActiveAndEnabled)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.isStopped = false;
            }
            //Desactivo mis otros componentes.
            _rb.useGravity = false;
            _rb.velocity = Vector3.zero;
            _mainCollider.enabled = false;
            _trail.DisableTrailEmission();

            //Busco todos los objetos que están al rededor y hacer que se mojen con baba.
            var findeds = Physics.OverlapSphere(transform.position, _explodeRange, _StaineableMask, QueryTriggerInteraction.Collide);
            foreach (var col in findeds)
            {
                var staineable = col.GetComponent<Staineable>();
                if (staineable != null)
                {
                    staineable.StainWithSlime();
                }
            }
        };
        explode.OnUpdate += () =>
        {
            print("Explotanding");
        };

        idle.OnEnter += (previousState) => 
        {
            _currentState = BabosoState.idle;
            //Seteo animacion
            _a_Walk = false;
            _a_Dead = false;
            _a_burning = false;

            if (startPatrolling)
            {
                _agent.isStopped = false;
            }
            else
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.isStopped = false;
            }

            _agent.enabled = true;
            //print(string.Format("{0}: Entró al estado Idle", gameObject.name));
        };
        idle.OnUpdate += () => checkPlayerOrClone();
        idle.OnExit += (nextState) => 
        {
            print(string.Format("{0}: Salió del estado Idle", gameObject.name));
        };

        patroll.OnEnter += (x) => 
        {
            _currentState = BabosoState.patroll;
            //Seteo la animacion.
            _a_Walk = true;

            if (_agent.isStopped)
                _agent.isStopped = false;

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

            checkPlayerOrClone();
        };
        patroll.OnExit += (x) => 
        {
            _a_Walk = false;
        };

        pursue.OnEnter += (x) => 
        {
            _currentState = BabosoState.pursue;
            _a_Walk = true;
            if (_agent.isStopped)
                _agent.isStopped = false;
        };
        pursue.OnUpdate += () => 
        {
            if(_currentTarget != null && _agent.destination != _currentTarget.position)
                _agent.SetDestination(_currentTarget.position);

            if (Vector3.Distance(_currentTarget.position, transform.position) < _attackRange)
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

            KillTarget();

            _agent.isStopped = true;
        };
        attack.OnExit += (nextState) =>
        {
            _a_attack = false;
            _agent.isStopped = false;
        };

        burning.OnEnter += (previousState) =>
        {
            StartCoroutine(Burn());
            _currentState = BabosoState.burning;
            //Prendemos la animación de quemado.
            _a_burning = true;
            _a_Walk = false;

            //En propósitos de debug.
            //var matColor = GetComponentInChildren<MeshRenderer>().material;
            //matColor.color = Color.red;
            _remainingBurnTime = _burnTime;

            //Dejamos de emitir baba.
            _trail.DisableTrailEmission();

            //Nos detenemos y reseteamos el camino.
            _agent.isStopped = true;
            _agent.ResetPath();
        };
        burning.OnUpdate += () => { };
        //burning.OnExit += (previousState) =>
        //{
        //    //Cuando se termina el tiempo de la animación saltamos a dead.
        //};

        falling.OnEnter += (x) =>
        {
            _currentState = BabosoState.falligTrap;

            _mainCollider.isTrigger = true;
            _rb.useGravity = true;
            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.enabled = false;
        };

        //think.OnEnter += (x) => 
        //{
        //    _currentState = BabosoState.think;
        //};
        //think.OnUpdate += () => 
        //{
        //    //Tomo desiciones... pero cuales?
        //    //Si mi enemigo esta muerto.
        //    if (_currentTarget == null && startPatrolling)
        //        state.Feed(BabosoState.patroll);
        //    else
        //        state.Feed(BabosoState.idle);
        //};
        //think.OnExit += (x) => 
        //{

        //};

        if (startPatrolling)
        {
            state = new GenericFSM<BabosoState>(patroll);
            _trail.EnableEmission();
        }
        else
        {
            state = new GenericFSM<BabosoState>(idle);
        }

#if UNITY_EDITOR
        state.Debug_Transitions = false; 
#endif
    }

    void Update()
    {
        state.Update();
    }

    //========================================== Member Funcs =================================================

    void checkPlayerOrClone()
    {
        if (_player != null && _sight.IsInSight(_player))
        {
            var targetState = _player.GetComponent<IDamageable<Damage, HitResult>>();
            if (targetState != null && targetState.IsAlive)
            {
                _currentTarget = _player;
                state.Feed(BabosoState.pursue);
            }
        }
        if (_playerClone != null && _sight.IsInSight(_playerClone))
        {
            var targetState = _playerClone.GetComponent<IDamageable<Damage, HitResult>>();
            if (targetState != null && targetState.IsAlive)
            {
                _currentTarget = _playerClone;
                state.Feed(BabosoState.pursue);
            }
        }
    }

    void KillTarget()
    {
        if (_currentTarget != null)
        {
            var killeable = _currentTarget.GetComponent<IDamageable<Damage, HitResult>>();
            if (killeable != null)
            {
                killeable.GetHit(new Damage() { instaKill = true });
            }
            else
                Debug.LogError("La cagaste, el target no es Damageable");
        }
        else
            Debug.LogError("La cagaste, el target es nulo");
    }

    public void ChangeStateTo(BabosoState input)
    {
        state.Feed(input);
    }

    //========================================== Sistema de Daño ==============================================

    public HitResult GetHit(Damage damage)
    {
        HitResult result = new HitResult()
        {
            conected = true,
            fatalDamage = true
        };

        //Debug.LogWarning(string.Format("{0} ha recibido un HIT", gameObject.name));
        if (damage.instaKill)
        {
            if (damage.type == DamageType.e_fire && _currentState != BabosoState.burning)
            {
                state.Feed(BabosoState.burning);
            }
            if (damage.type == DamageType.blunt && _currentState != BabosoState.explode)
            {
                state.Feed(BabosoState.explode);
            }
            if (damage.type == DamageType.piercing && _currentState != BabosoState.dead)
            {
                state.Feed(BabosoState.dead);
            }
        }
        else
        {
            health -= damage.Ammount;
        }
        return result;
    }
    public void FeedDamageResult(HitResult result) { }
    public Damage GetDamageStats()
    {
        return _damageState;
    }

    //===================================== Animation Events ===================================================

    //Fases de Ataques --> StartUp, Active, Recovery
    //Por ahora el juego es Instakill, asi que cuando un enemigo te alcanza, te golpea y tu mueres.

    void AV_Attack_End()
    {
        var AttackTarget = _currentTarget.GetComponent<IDamageable<Damage, HitResult>>();
        if (AttackTarget != null)
        {
            if (AttackTarget.IsAlive)
            {
                if (Vector3.Distance(transform.position, _currentTarget.position) < _attackRange)
                    state.Feed(BabosoState.attack);
                else
                    state.Feed(BabosoState.pursue);
            }
            else
            {
                _currentTarget = null;
                if (startPatrolling)
                    state.Feed(BabosoState.patroll);
                else
                    state.Feed(BabosoState.idle);
            }
        }
    }
    void AV_Burning_End()
    {
        state.Feed(BabosoState.dead);
        Debug.LogWarning("AnimEvent: BurningEnd");
    }

    IEnumerator Burn()
    {
        burnParticles[0].SetActive(true);
        yield return new WaitForSeconds(0.1f);
        burnParticles[1].SetActive(true);
    }

    //===================================== DEBUG ===============================================================

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //Rango de ataque
        Gizmos.color = DEBUG_AttackRangeColor;
        Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        Gizmos.color = DEBUG_ExplodeRangeColor;
        Gizmos.DrawWireSphere(transform.position, _explodeRange);
    }
#endif
}
