using System;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;
using IA.LineOfSight;
using IA.FSM;
using IA.PathFinding;
using Core.SaveSystem;

[RequireComponent(typeof(Animator), typeof(LineOfSightComponent), typeof(PathFindSolver)), RequireComponent(typeof(Collider))]
public abstract class BaseNPC : MonoBehaviour, IDamageable<Damage, HitResult>, ILivingEntity
{
    //================================ Basic Variables ===============================================
    [Header("============= Common Stats ===============================")]
    [SerializeField] protected float _health      = 100f;
    [SerializeField] protected float _maxHealth   = 100f;
    [SerializeField] protected float _attackRange = 2f;
    [SerializeField] protected float _minDetectionRange = 3f;

    [Header("Scene Data")]
    public int sceneID = -1;

    [Space(), Header("Modificadores de Daño")]
    [SerializeField] protected DamageModifier[] Weaknesses; //Aumentan el daño ingresante.
    [SerializeField] protected DamageModifier[] resistances;//reducen el daño ingereante.
    [SerializeField] protected Damage _defaultDamage = new Damage();

    [SerializeField] protected Collider _mainCollider = null;
    [SerializeField] protected Controller _player                          = null;
    [SerializeField] protected ClonBehaviour _playerClone                  = null;
    [SerializeField] protected IDamageable<Damage, HitResult> _attackTarget = null;

    //=================================== Components =================================================

    [SerializeField]
    protected Transform _lineOfSghtOrigin = null;
    protected Animator _anims             = null;
    protected Rigidbody _rb               = null;
    protected PathFindSolver _solver      = null;
    protected LineOfSightComponent _sight = null;

    //=================================== State Machine ==============================================

    protected FiniteStateMachine<CommonState> _states = new FiniteStateMachine<CommonState>();
    public void ChangeStateTo(CommonState input)
    {
        _states.Feed(input);
    }

    //=================================== DEBUGGING ==================================================

#if UNITY_EDITOR
    [Header("------------------- DEBUG --------------------------")]
    [SerializeField] protected bool debugThisUnit = false;
#endif

    //================================ ILivingEntity =================================================

    public Action<Collider> OnEntityDead = delegate { };

    public void SubscribeToLifeCicleDependency(Action<Collider> OnEntityDead)
    {
        this.OnEntityDead += OnEntityDead;
    }
    public void UnsuscribeToLifeCicleDependency(Action<Collider> OnEntityDead)
    {
        this.OnEntityDead -= OnEntityDead;
    }

    //================================ Damage System =================================================

    public bool IsAlive => _health > 0;

    public virtual HitResult GetHit(Damage damage)
    {
        throw new NotImplementedException();
    }
    public virtual void FeedDamageResult(HitResult result)
    {
        throw new NotImplementedException();
    }
    public virtual Damage GetDamageStats()
    {
        throw new NotImplementedException();
    }
    public virtual void GetStun(Vector3 AgressorPosition, int PosibleKillingMethod)
    {
        throw new NotImplementedException();
    }

    //================================ Interaction System ============================================

    public virtual void OnIgnite(object[] optionalParams)
    {
        //Si recibe daño igniteante, este puede prenderse fuego.
        _states.Feed(CommonState.burning);
    }
    public virtual void OnHitWithRock(object[] optionalParams)
    {
        //Si es atacado por una roca, entra en rageMode.
        print("Me han pegado con una roca, hijos de puta!");
        _states.Feed(CommonState.rage);
    }

    //=============================== Common Utility Functions =======================================

    /// <summary>
    /// Mueve a la unidad hasta un nodo determinado con la velocidad específicada.
    /// </summary>
    /// <param name="targetNode">Nodo objetivo al que se quiere moverse.</param>
    /// <param name="movementSpeed">Velocidad del movimiento</param>
    /// <returns>Retorna false si no alcanzó al objetivo, de lo contrario, retorna verdadero si alcanzó al objetivo dentro del treshold correspondiente.</returns>
    protected bool MoveToNode(Node targetNode, float movementSpeed)
    {
        Vector3 targetDir = (targetNode.transform.position - transform.position).normalized;
        transform.forward = (targetDir.YComponent(0)); //Orientación
        transform.position += targetDir * movementSpeed * Time.deltaTime; //Cambio de posición.

        //Chequeo la distancia al objetivo actual
        return Vector3.Distance(targetNode.transform.position, transform.position) < _solver.ProximityTreshold;
    }
    /// <summary>
    /// Resetea el estado de la vida de esta entidad a su estado originial.
    /// </summary>
    protected void ResetSetUp()
    {
        _health = _maxHealth;
    }
    /// <summary>
    /// Cambia la orientación del NPC para que mire en dirección de su objetivo de ataque.
    /// </summary>
    protected void LookAtAttackTarget()
    {
        if (_attackTarget != null && _attackTarget.IsAlive)
            transform.forward = (_attackTarget.transform.position - transform.position).normalized;
    }
    /// <summary>
    /// Paraliza al objetivo de ataque si este existe y no es nulo.
    /// </summary>
    protected void StunTarget()
    {
        if (_attackTarget != null && _attackTarget.IsAlive)
            _attackTarget.GetStun(transform.position, 1);
    }
    /// <summary>
    /// Chequea si el jugador y su clon estan en rango de visión.
    /// Si se cumple la condición, setea el más cercano como target de Ataque.
    /// </summary>
    /// <returns>Retorna Verdadero si el jugador o el clon fueron encontrados!</returns>
    protected bool checkForPlayerOrClone()
    {
#if UNITY_EDITOR
        if (debugThisUnit)
        {
            print("Checking Vision!");
        }
#endif

        IDamageable<Damage, HitResult> closerTarget = null;
        float distToPlayer = float.MaxValue;
        float distToClone = float.MaxValue;

        if (_player != null && _player.IsAlive)
        {
            distToPlayer = (_player.transform.position - transform.position).magnitude;
            Vector3 customLOSTargetDir = (_player.getLineOfSightTargetPosition() - _lineOfSghtOrigin.position).normalized;
            if (_sight.IsInSight(_lineOfSghtOrigin.position, customLOSTargetDir, _player.transform) || distToPlayer < _minDetectionRange)
                closerTarget = _player;
        }

        if (_playerClone != null && _playerClone.IsAlive && _playerClone.isActiveAndEnabled)
        {
            distToClone = (_playerClone.transform.position - transform.position).magnitude;
            Vector3 customLOSTargetDir = (_playerClone.getLineOfSightTargetPosition() - _lineOfSghtOrigin.position).normalized;
            if (_sight.IsInSight(_lineOfSghtOrigin.position, customLOSTargetDir, _playerClone.transform) && distToClone < distToPlayer ||
                    distToClone < _minDetectionRange)
                closerTarget = _playerClone;
        }

        if (closerTarget != null)
        {
            var currentTarget = closerTarget;
            if (currentTarget != null)
            {
                _attackTarget = currentTarget;
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
    protected bool TargetIsInAttackRange()
    {
        if (_attackTarget == null || !_attackTarget.IsAlive) return false;
        //Si la distancia de ataque es menor al rago de ataque.
        float DistanceToTarget = (_attackTarget.transform.position - transform.position).magnitude;
        return DistanceToTarget < _attackRange;
    }
    /// <summary>
    /// Retorna el nodo mas cercano al objetivo actual de Ataque.
    /// </summary>
    /// <returns>Null si no existe objetivo o si ya esta muerto.</returns>
    protected Node getCloserNodeToAttackTarget()
    {
        if ((_attackTarget != null && _attackTarget.IsAlive))
        {
            //Calculamos el nodo al que nos queremos mover.
            Node TargetNode;
            var isRock = _attackTarget.GetComponent<DestructibleRock>() != null;
            if (isRock)
            {
                TargetNode = PathFindSolver.getCloserWalkableNodeInGraph(_attackTarget.transform.position, FindObjectOfType<NodeGraphBuilder>());
            }
            else TargetNode = _solver.getCloserNode(_attackTarget.transform.position);
            return TargetNode;
        }
        return null;
    }
    protected IDamageable<Damage, HitResult> getAttackTarget()
    {
        return _attackTarget;
    }

    //======================================= Unity Engine ===========================================

    protected virtual void Awake()
    {
        _health = _maxHealth;

        //Inicialización de componentes básicos.
        _rb = GetComponent<Rigidbody>();
        _sight = GetComponent<LineOfSightComponent>();
        _solver = GetComponent<PathFindSolver>();
        _anims = GetComponent<Animator>();

        if (!_mainCollider)
            _mainCollider = GetComponent<Collider>();

        //Posicionamiento inicial.
        Node _currentCloserNode = _solver.getCloserNode(transform.position);
        transform.position = _currentCloserNode.transform.position;

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
    }
    protected virtual void Update()
    {
        //Update de la State Machine.
        _states.Update();
    }
    private void OnDestroy()
    {
        OnEntityDead(_mainCollider);
    }
    public virtual void LoadEnemyData(EnemyData enemyData)
    {
        print($"{gameObject.name} has loaded his data.");
    }
    public virtual EnemyData getEnemyData()
    {
        //Esto hay que overraidarlo en cada tipo de enemigo!!
        return new EnemyData();
    }
}
