using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.PathFinding;

[System.Serializable]
public class cmd_Move : IQueryComand
{
    public Func<Vector3, bool> _moveFunction = delegate { return false; };
    public Func<Vector3, bool> _checkCondition = delegate { return false; };
    public Action _dispose = delegate { };
    public Queue<Node> _pathToTarget = new Queue<Node>();

    Vector3 _targetPosition = Vector3.zero;

    public cmd_Move(Vector3 targetPosition, Queue<Node> pathToTarget, Func<Vector3, bool> moveFunction, Func<Vector3, bool> checkCondition, Action dispose)
    {
        _targetPosition = targetPosition;
        _pathToTarget = new Queue<Node>(pathToTarget);
        _moveFunction = moveFunction;
        _checkCondition = checkCondition;
        _dispose = dispose;
    }

    public bool completed { get; private set; } = false;

    public void setUp()
    {
        //Esto lo tengo al pepe por ahora.
    }

    public void Execute()
    {
        completed = _checkCondition(_targetPosition);
        if (completed)
            _dispose();
        else
        {
            //Seleccionamos el nodo al que nos queremos dirigir.

            //Move Function Retorna true, cuando la distancia al objetivo despues del movimiento es menor al treshold.
            _moveFunction(_targetPosition);

            //Si el move Retorna true, entonces actualizamos nuestro siguiente nodo.
        }

        //MonoBehaviour.print($"move comand Executing, status {completed ? "Completed" : "On Going"}");
    }
    public void Cancel() { }
}
