using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cmd_Activate : IQueryComand
{
    Action _dispose = delegate { };
    OperationOptions operation;
    IInteractable target;

    public bool completed { get; private set; } = false;

    public cmd_Activate(OperationOptions operation, IInteractable target, Action dispose)
    {
        this.operation = operation;
        this.target = target;
        _dispose = dispose;
    }

    public void Update()
    {
        //Ejecuto el comando.
        target.Operate(operation);
        _dispose();
        MonoBehaviour.print("COMANDO ACTIVAR");
    }

    public void setUp()
    {
        MonoBehaviour.print("Setup Query Comand: Activate");
    }
}
