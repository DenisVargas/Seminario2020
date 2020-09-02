using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.PathFinding;

[System.Serializable]
public class cmd_Move : IQueryComand
{
    public Func<Node, bool> _moveFunction = delegate { return false; };
    public Func<Node, bool> _checkCondition = delegate { return false; };
    public Action _dispose = delegate { };
    public Action OnChangePath = delegate { };
    public Queue<Node> _pathToTarget = new Queue<Node>();

    Node _currentTarget = null;
    Node _targetNode = null;

    public cmd_Move(Node TargetNode, Queue<Node> pathToTarget, Action OnChangePath, Func<Node, bool> moveFunction, Func<Node, bool> checkCondition, Action Dispose)
    {
        _targetNode = TargetNode;
        _pathToTarget = new Queue<Node>(pathToTarget);
        this.OnChangePath = OnChangePath;
        _currentTarget = _pathToTarget.Dequeue();
        _moveFunction = moveFunction;
        _checkCondition = checkCondition;
        _dispose = Dispose;
    }

    public bool completed { get; private set; } = false;
    public bool isReady { get; private set; } = false;
    public bool cashed => false;

    public void SetUp()
    {
        //Move no es un comando Casheado. Por lo que tiene una ejecución contínua.
        OnChangePath();
        isReady = true;
    }
    public void Execute()
    {
        completed = _checkCondition(_targetNode);
        if (completed)
            _dispose();
        else
        {
            //Move Function Retorna true, cuando la distancia al objetivo despues del movimiento es menor al treshold.
            if(_moveFunction(_currentTarget) && _pathToTarget.Count > 0)//Si el move Retorna true, entonces actualizamos nuestro siguiente nodo.
                _currentTarget = _pathToTarget.Dequeue();
        }

        //MonoBehaviour.print($"move comand Executing, status {completed ? "Completed" : "On Going"}");
    }
    public void Cancel() { }
}
