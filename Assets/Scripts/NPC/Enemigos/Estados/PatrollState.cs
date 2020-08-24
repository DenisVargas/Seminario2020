using System;
using UnityEngine;
using IA.FSM;
using IA.PathFinding;
using System.Collections.Generic;

[RequireComponent(typeof(PathFindSolver), typeof(Waypoint))]
public class PatrollState : State
{
    public Func<bool> checkForPlayer = delegate { return false; };
    public Func<Node, float, bool> moveToNode = delegate { return false; };

    [SerializeField] Waypoint patrolPoints = null;
    [SerializeField] float _patrollSpeed   = 5f;
    [SerializeField] float _stopTime       = 1.5f;

    bool _stoping            = false;
    int _PositionsMoved      = 0;
    float _remainingStopTime = 0.0f;

    PathFindSolver _solver = null;
    List<Node> _waypointNodes = new List<Node>();
    Node _nextNode         = null;

    public override void Begin()
    {
        //Obtengo referencias.
        if (_solver == null)
            _solver = GetComponent<PathFindSolver>();

        _waypointNodes = new List<Node>();
        ConvertWaypointsToNodeWaypoints();

        //Seteo la animacion.
        _anims.SetBool("Walking", true);
        _remainingStopTime = _stopTime;

        _stoping = false;

        //Calculo el camino.
        _solver.SetOrigin(transform.position)
               .SetTarget(_nextNode)
               .CalculatePathUsingSettings();
    }

    public override void Execute()
    {
        //Si la distancia es menor al treshold
        if (Vector3.Distance(_nextNode.transform.position, transform.position) < _solver.ProximityTreshold)
        {
            _PositionsMoved++;
            if (patrolPoints.points.Count < _PositionsMoved)
                _PositionsMoved = 0;
            //_stoping = _positionsmoved >= _tostoppositions;
            _nextNode = _solver.getCloserNode(patrolPoints.getNextPosition(_PositionsMoved));
            _stoping = true;
        }
        else
        {
            //Me muevo hacia el objetivo actual.
            moveToNode(_nextNode, _patrollSpeed);
        }

        //Timing.
        if (_stoping)
        {
            //Debug.Log("entre al stopping");
            _remainingStopTime -= Time.deltaTime;

            if (_remainingStopTime <= 0)
            {
                //Debug.Log("Entre Al desstoping");
                _remainingStopTime = _stopTime;
                _stoping = false;
            }
        }

        checkForPlayer();
    }

    public override void End()
    {
        _anims.SetBool("Walking", false);
    }

    void ConvertWaypointsToNodeWaypoints()
    {
        //Convierte los waypoints en ruta de nodos.
        int _index = 0;
        foreach (var item in patrolPoints.points)
        {
            _waypointNodes.Add(_solver.getCloserNode(patrolPoints.getNextPosition(_index)));
            _index++;
        }
    }
}
