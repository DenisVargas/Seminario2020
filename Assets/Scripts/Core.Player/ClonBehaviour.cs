using System;
using UnityEngine;
using Core.DamageSystem;
using IA.PathFinding;

public class ClonBehaviour : MonoBehaviour, IDamageable<Damage, HitResult>
{
    event Action OnRecast = delegate { };
    public void RegisterRecastDependency(Action onEntityDied)
    {
        OnRecast += onEntityDied;
    }
    public void UnregisterRecastDependency(Action onEntityDied)
    {
        OnRecast -= onEntityDied;
    }

    Animator _anims = null;
    PathFindSolver _solver = null;

    [SerializeField] float _moveSpeed = 3;
    [SerializeField] float _maxLifeTime = 20f;

    float remainingLifeTime = 0f;
    bool canMove = false;

    Node _current = null;
    Node _Next = null;
    Node _targetNode = null;

    public bool IsActive
    {
        get => gameObject.activeSelf;
    }

    bool _a_Walking
    {
        get => _anims.GetBool("walking");
        set => _anims.SetBool("walking", value);
    }

    public bool IsAlive { get; private set; } = (true);

    // Start is called before the first frame update

    public void Awake()
    {
        _anims = GetComponent<Animator>();
        _solver = GetComponent<PathFindSolver>();
        remainingLifeTime = _maxLifeTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove && _Next != null)
        {
            //Me muevo a mi objetivo. Si completo el path terminé de moverme.
            if (Move(_Next))
            {
                if (_Next == _targetNode)
                {
                    _a_Walking = false;
                    _Next = null;
                    return;
                }

                _current = _Next;
                if (_solver.currentPath.Count > 0)
                    _Next = _solver.currentPath.Dequeue();
            }
        }

        if (remainingLifeTime > 0)
        {
            //print($"Tiempo de vida restante es {remainingLifeTime}");
            remainingLifeTime -= Time.deltaTime;
        }
        else
            UncastClon();
    }

    public void SetMovementDestinyPosition(Node destinyPosition)
    {
        if (!canMove) return;
        _targetNode = destinyPosition;

        //Calculo las posiciones!
        _solver.SetOrigin(_solver.getCloserNode(transform.position))
               .SetTarget(destinyPosition)
               .CalculatePathUsingSettings();

        _current = _solver.currentPath.Dequeue();
        _Next = _solver.currentPath.Dequeue();
    }
    public bool Move(Node targetNode)
    {
        Vector3 dirToTarget = (targetNode.transform.position - transform.position).normalized;
        transform.forward = dirToTarget;

        if (!_a_Walking)
            _a_Walking = true;

        transform.position += dirToTarget * _moveSpeed * Time.deltaTime;
        float distance = Vector3.Distance(transform.position, targetNode.transform.position);
        //print($"remaining distance is {distance}");
        return distance < _solver.ProximityTreshold;
    }
    public void InvokeClon(Node node, Vector3 forward)
    {
        transform.position = node.transform.position;
        transform.forward = forward;
        IsAlive = true;
        canMove = false;
        gameObject.SetActive(true);
    }
    public void UncastClon()
    {
        //print("Recast Clon!");
        _solver.currentPath.Clear();
        gameObject.SetActive(false);
        remainingLifeTime = _maxLifeTime;
        canMove = false;
        IsAlive = false;

        _current = null;
        _Next = null;
        _targetNode = null;

        OnRecast();
    }

    //================================ Animation Events ======================================

    void AV_startedInvoke()
    {
        canMove = false;
    }
    void AV_finishedInvoke()
    {
        canMove = true;
    }

    //================================ Damage System =========================================

    public HitResult GetHit(Damage damage)
    {
        HitResult result = new HitResult(true);

        if (damage.type == DamageType.blunt || damage.type == DamageType.piercing)
        {
            result.fatalDamage = true;
            UncastClon(); //Podriamos animarlo pero ALV.
        }

        return result;
    }

    public void FeedDamageResult(HitResult result) { }

    public Damage GetDamageStats()
    {
        return new Damage { Ammount = 0, criticalMultiplier = 0, instaKill = false, type = DamageType.piercing };
    }

    public void GetStun(Vector3 AgressorPosition, int PosibleKillingID)
    {
        canMove = false; //Bloqueo el camino we.
        //Quizás reproducir una animación.
    }
}
