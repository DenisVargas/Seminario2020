using IA.FSM;
using IA.PathFinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;
using Core.SaveSystem;

[RequireComponent(typeof(PathFindSolver))]
public class PursueState : State
{
    public Action StopMovement = delegate { };
    public Action<Node> OnUpdateCurrentNode = delegate { };
    public Func<Node,float, bool> MoveToTarget = delegate { return false; };
    public Func<bool> checkDistanceToTarget = delegate { return false; };
    public Func<Node> getDestinyNode = delegate { return null; };
    public Func<bool> TargetIsActiveAndAlive = delegate { return true; };
    public Func<IDamageable<Damage, HitResult>> getTarget = delegate { return null; };

    [SerializeField] int _pursuerID = -1;
    [SerializeField] float _pursueMovementSpeed = 3f;
    [SerializeField] float _minDistanceToAttack = 3f;

    IDamageable<Damage, HitResult> _player = null;
    IDamageable<Damage, HitResult> _clon = null;
    PathFindSolver _solver = null;
    Node _current = null;
    Node _next = null;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _minDistanceToAttack);
    }

    public PursueState Set(Controller player, ClonBehaviour clon, int pursuerID)
    {
        this._player = player;
        this._clon = clon;
        this._pursuerID = pursuerID;
        return this;
    }

    public override void Begin()
    {
        _anims.SetBool("Walking", true);
        _solver = GetComponent<PathFindSolver>();

        //Obtengo el current target.
        var target = getTarget();
        if (target != null)
        {
            if (target == _player) //Si es el jugador...
            {
                var c = target.GetComponent<Controller>();
                c.OnMovementChange += RecalculateValidPathToTarget; //guardo el evento de recalculado.
            }
        }

        //Cálculo del camino inicial.
        if (_current == null && _next == null)
        {
            _current = _solver.getCloserNode(transform.position);
            RecalculateValidPathToTarget();
        }

        Level.registerPursuer(_pursuerID);
    }

    public override void Execute()
    {
        if (!TargetIsActiveAndAlive())
        {
            SwitchToState(CommonState.idle);
            return;
        }

        if (checkDistanceToTarget())
        {
            SwitchToState(CommonState.attack);
            return;
        }

        //Me muevo en dirección al objetivo.
        //Si alcance el objetivo intermedio, descarto el siguiente nodo.
        if (MoveToTarget(_next, _pursueMovementSpeed))
        {
            _current = _next;
            RecalculateValidPathToTarget();
        }
    }

    public override void End()
    {
        StopMovement();
        _anims.SetBool("Walking", false);
        _solver.currentPath.Clear();
        _current = null;
        _next = null;

        //Obtengo el current target.
        var target = getTarget();
        if (target != null)
        {
            var player = target.GetComponent<Controller>();
            if (player != null) //Si es el jugador...
            {
                player.OnMovementChange -= RecalculateValidPathToTarget; //guardo el evento de movimiento
            }
        }

        Level.unregisterPursuer(_pursuerID);
    }

    void RecalculateValidPathToTarget()
    {
        _solver.SetOrigin(_current);

        Node targetNode = getDestinyNode();
        if (targetNode != null)
        {
            _solver.SetTarget(getDestinyNode())
                   .CalculatePathUsingSettings();

            _current = _solver.currentPath.Dequeue();
            OnUpdateCurrentNode(_current);
            _next = _solver.currentPath.Peek();
        }
    }
}
