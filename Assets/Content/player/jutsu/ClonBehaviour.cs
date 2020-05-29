using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ClonBehaviour : MonoBehaviour
{

    MouseContextTracker _mtracker;
    Animator _anims;
    NavMeshAgent _agent = null;
    bool CanMove;
    [SerializeField] float _movementTreshold = 0.18f;
    Vector3 _currentTargetPos;

    bool _a_Walking
    {
        get => _anims.GetBool("walking");
        set => _anims.SetBool("walking", value);
    }
    // Start is called before the first frame update

    void Awake()
    {
        _mtracker = GetComponent<MouseContextTracker>();
        _anims = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }
   

    // Update is called once per frame
    void Update()
    {
        if (CanMove) {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            MouseContext _mouseContext = _mtracker.GetCurrentMouseContext();
            if (!_mouseContext.validHit) return;
            Debug.Log("entre");
            MoveToTarget(_mouseContext.hitPosition);

        }
        if (transform.position == _agent.destination)
        {
            _a_Walking = false;
        }
        }
        

    }
    public void MoveToTarget(Vector3 destinyPosition)
    {
        Vector3 _targetForward = (destinyPosition -transform.position).normalized.YComponent(0);
        transform.forward = _targetForward;

        if (!_a_Walking)
            _a_Walking = true;

        _agent.destination = destinyPosition;
    }
    void canMove(int canorcant)
    {
        if (canorcant == 0)
            CanMove = false;
        else
            CanMove = true;
    }
   


}
