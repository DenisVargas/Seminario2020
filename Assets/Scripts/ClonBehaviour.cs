using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Core.DamageSystem;

public class ClonBehaviour : MonoBehaviour, IDamageable<Damage, HitResult>
{
    public event Action OnRecast = delegate { };

    Animator _anims = null;
    NavMeshAgent _agent = null;
    Vector3 _currentTargetPosition;

    [SerializeField] float _maxLifeTime;
    [SerializeField] float _ClonMovementTreshold = 0.1f;

    float remainingLifeTime = 0f;
    bool canMove = false;

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
        _agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            float dst = Vector3.Distance(transform.position, _currentTargetPosition);
            if (dst < _ClonMovementTreshold)
            {
                _a_Walking = false;
                _agent.isStopped = true;
                _agent.ResetPath();
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
        _ClonMovementTreshold = MovementTreshold;
        canMove = false;
    }
    public void SetMovementDestinyPosition(Vector3 destinyPosition)
    {
        _currentTargetPosition = destinyPosition;
        Vector3 _targetForward = (destinyPosition -transform.position).normalized.YComponent(0);
        transform.forward = _targetForward;

        _a_Walking = true;

        if (_agent.isStopped)
            _agent.isStopped = false;
        _agent.destination = destinyPosition;
    }
    public void InvokeClon(Vector3 position, Vector3 forward)
    {
        transform.position = position;
        transform.forward = forward;
        IsAlive = true;
        gameObject.SetActive(true);
    }
    public void RecastClon()
    {
        gameObject.SetActive(false);
        remainingLifeTime = _maxLifeTime;
        canMove = false;
        IsAlive = false;
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

    public void GetStun()
    {
        _agent.isStopped = true;
        _agent.ResetPath();
    }
}
