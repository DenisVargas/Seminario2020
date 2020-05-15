using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class cmd_Move : IQueryComand
{
    public Action<Vector3> _moveFunction = delegate { };
    public Func<Vector3, bool> _checkCondition = delegate { return false; };
    public Action _dispose = delegate { };

    Vector3 _targetPosition = Vector3.zero;

    public cmd_Move(Vector3 targetPosition, Action<Vector3> moveFunction, Func<Vector3, bool> checkCondition, Action dispose)
    {
        _targetPosition = targetPosition;
        _moveFunction = moveFunction;
        _checkCondition = checkCondition;
        _dispose = dispose;
    }

    public bool completed { get; private set; } = false;

    public void setUp()
    {
        //Esto lo tengo al pepe por ahora.
    }

    public void Update()
    {
        completed = _checkCondition(_targetPosition);
        if (!completed)
            _moveFunction(_targetPosition);
        else
            _dispose();

        //MonoBehaviour.print(string.Format("move comand Executing, status {0}", completed ? "Completed" : "On Going"));
    }

}
