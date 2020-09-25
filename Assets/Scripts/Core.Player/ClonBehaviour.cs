using System;
using UnityEngine;
using Core.DamageSystem;
using IA.PathFinding;

public class ClonBehaviour : MonoBehaviour, IDamageable<Damage, HitResult>
{
    public event Action OnRecast = delegate { };

    Animator _anims = null;
    PathFindSolver _solver = null;

    [SerializeField] float _moveSpeed = 3;
    [SerializeField] float _maxLifeTime;
    [SerializeField] float _movementTreshold = 0.1f;

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
                    canMove = false;
                    _a_Walking = false;
                    return;
                }

                _current = _Next;
                _Next = _solver.currentPath.Dequeue();
            }
        }

        if (remainingLifeTime > 0)
            remainingLifeTime -= Time.deltaTime;
        else
            RecastClon();
    }
    public void SetState(float maxLifeTime, float MovementTreshold)
    {
        _maxLifeTime = maxLifeTime;
        remainingLifeTime = maxLifeTime;
        _movementTreshold = MovementTreshold;
        canMove = false;
    }
    public void SetMovementDestinyPosition(Node destinyPosition)
    {
        canMove = true;
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
        return Vector3.Distance(transform.position, targetNode.transform.position) <= _movementTreshold;
    }
    public void InvokeClon(Node node, Vector3 forward)
    {
        transform.position = node.transform.position;
        transform.forward = forward;
        IsAlive = true;
        canMove = false;
        gameObject.SetActive(true);
    }
    public void RecastClon()
    {
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
        HitResult result = new HitResult()
        {
            conected = true,
            fatalDamage = true
        };

        RecastClon(); //Podriamos animarlo pero ALV.
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
