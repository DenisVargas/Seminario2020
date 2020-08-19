using IA.FSM;
using IA.PathFinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathFindSolver))]
public class PursueState : State
{
    public Action StopMovement = delegate { };
    public Func<Node,float, bool> MoveToTarget = delegate { return false; };
    public Func<bool> checkDistanceToTarget = delegate { return false; };
    public Func<Node> getDestinyNode = delegate { return null; };

    [SerializeField] float _pursueMovementSpeed = 3f;
    [SerializeField] float _minDistanceToAttack = 3f;

    PathFindSolver _solver = null;
    Node _current = null;
    Node _next = null;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _minDistanceToAttack);
    }

    public override void Begin()
    {
        _anims.SetBool("Walking", true);
        _solver = GetComponent<PathFindSolver>();
    }

    public override void Execute()
    {
        //Cada Frame me fijo cual es el nodo en el que estoy actualmente y hacia donde voy.
        if (_current == null && _next == null)
        {
            _current = _solver.getCloserNode(transform.position);
            CalculateValidPathToTarget();
            _solver.currentPath.Dequeue();
            _next = _solver.currentPath.Peek();
        }

        //Magia de recálculo de path, si alguna condición cambia.
        //Solo deberíamos recalcular el camino si la posición del objetivo es distinto del ultimo.

        //Me muevo en dirección al objetivo.
        //Si alcance el objetivo intermedio, descarto el siguiente nodo.
        if (MoveToTarget(_next, _pursueMovementSpeed))
        {
            _current = _solver.currentPath.Dequeue();
            _next = _solver.currentPath.Peek();
        }

        if (checkDistanceToTarget())
            SwitchToState(CommonState.attack);
    }

    public override void End()
    {
        StopMovement();
        _anims.SetBool("Walking", false);
        _solver.currentPath.Clear();
        _current = null;
        _next = null;
    }

    void CalculateValidPathToTarget()
    {
        _solver.SetOrigin(_current);

        Node targetNode = getDestinyNode();
        if (targetNode != null)
        {
            _solver.SetTarget(getDestinyNode())
                   .CalculatePathUsingSettings();
        }
    }
}
