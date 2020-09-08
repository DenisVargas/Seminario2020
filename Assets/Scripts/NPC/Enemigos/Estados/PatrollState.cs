using System;
using System.Collections.Generic;
using UnityEngine;
using IA.FSM;
using IA.PathFinding;
using IA.Waypoints;

[RequireComponent(typeof(PathFindSolver), typeof(NodeWaypoint))]
public class PatrollState : State
{
    public Func<bool> checkForPlayer = delegate { return false; };
    public Func<Node, float, bool> moveToNode = delegate { return false; };

    [SerializeField] NodeWaypoint patrolPoints = null;
    [SerializeField] float _patrollSpeed   = 5f;
    [SerializeField] float _stopTime       = 1.5f;

    bool _stoping            = false;
    bool _waitForPosibleRoute = false;
    int _PositionsMoved      = 0;
    float _remainingStopTime = 0.0f;

    PathFindSolver _solver     = null;

    List<Node> _waypointNodes  = new List<Node>();
    Node _nextWayPointNode = null;
    Node _nextNode         = null;
    Node _currentNode      = null;

    List<Node> primaryRoute = new List<Node>();
    List<Node> alternativeRoute = new List<Node>();

    public override void Begin()
    {
        //Obtengo referencias.
        if (_solver == null)
            _solver = GetComponent<PathFindSolver>();
        if (_anims == null)
            _anims = GetComponent<Animator>();
        if (patrolPoints == null)
            patrolPoints = GetComponent<NodeWaypoint>();

        _waypointNodes = new List<Node>();
        foreach (var item in patrolPoints.points)
            _waypointNodes.Add(item);

        //Seteo la animacion.
        _anims.SetBool("Walking", true);
        _remainingStopTime = _stopTime;

        _stoping = false;

        //Asigno los nodos de referencia.
        _currentNode = _solver.getCloserNode(transform.position); //Empezamos y consumimos el primero.
        if (_currentNode == _waypointNodes[0])
            _PositionsMoved = 1;
        _nextWayPointNode = _waypointNodes[_PositionsMoved];

        //Calculo el camino.
        _solver.SetOrigin(_currentNode)
               .SetTarget(_nextWayPointNode)
               .CalculatePathUsingSettings();

        //Me aseguro de que el próximo nodo no sea el mismo que el current.
        _nextNode = _solver.currentPath.Dequeue();
        if (_nextNode == _currentNode)
            _nextNode = _solver.currentPath.Dequeue();
    }

    public override void Execute()
    {
        if (checkForPlayer())
        {
            SwitchToState(CommonState.pursue);
            return;
        }

        if (_stoping)
        {
            _remainingStopTime -= Time.deltaTime;

            if (_remainingStopTime <= 0)
            {
                _remainingStopTime = _stopTime;
                _stoping = false;
            }
            return;
        }

        //Si la distancia es menor al treshold
        if (Vector3.Distance(_nextNode.transform.position, transform.position) < _solver.ProximityTreshold)
        {
            //Si alcanzamos nuestro nextPoint y este es el siguiente nodo del Waypoint.
            if (_nextNode == _nextWayPointNode)
            {
                _PositionsMoved++;

                if (_PositionsMoved >= patrolPoints.points.Count)
                    _PositionsMoved = 0;

                _currentNode = _nextWayPointNode;
                _nextWayPointNode = _waypointNodes[_PositionsMoved];


                _solver.SetOrigin(_currentNode)
                       .SetTarget(_nextWayPointNode)
                       .CalculatePathUsingSettings();

                if (_solver.currentPath.Count == 0)
                {
                    _waitForPosibleRoute = true;
                    return;
                }

                _nextNode = _solver.currentPath.Dequeue();
                _nextNode = _solver.currentPath.Dequeue();

                _stoping = true;
            }
            else
            {
                if (_solver.currentPath.Count == 0)
                    return;
                _nextNode = _solver.currentPath.Dequeue();//Si alcanzamos el siguiente nodo del camino actual.
            }
        }

        moveToNode(_nextNode, _patrollSpeed); //Me muevo hacia el objetivo actual.
    }

    public override void End()
    {
        _anims.SetBool("Walking", false);
    }
}
