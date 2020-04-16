using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class cmd_Move : IQueryComand
{
    public Action<Vector3> _moveFunction = delegate { };
    public Func<bool> _checkCondition = delegate { return false; };

    Vector3 _targetPosition = Vector3.zero;
    Transform origin = null;

    public cmd_Move(Vector3 targetPosition, Action<Vector3> moveFunction, Func<bool> checkCondition)
    {
        _targetPosition = targetPosition;
        _moveFunction = moveFunction;
        _checkCondition = checkCondition;
    }

    public bool completed { get; private set; } = false;

    public void setUp()
    {
        //Esto lo tengo al pepe por ahora.
    }

    public void Update()
    {
        _moveFunction(_targetPosition);

        completed = _checkCondition();
        MonoBehaviour.print(string.Format("move comand Executing, state {0}", completed));
    }

}
