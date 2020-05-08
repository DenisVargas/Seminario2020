using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cmd_Activate : IQueryComand
{
    Action OnOperate = delegate { };
    Action _dispose = delegate { };
    OperationOptions operation;
    IInteractable target;

    public bool completed { get; private set; } = false;

    public cmd_Activate(OperationOptions operation, IInteractable target,Action OnOperate, Action dispose)
    {
        this.operation = operation;
        this.target = target;
        this.OnOperate = OnOperate;
        _dispose = dispose;
    }

    public void Update()
    {
        //Ejecuto el comando.
        OnOperate();
        target.Operate(operation);
        _dispose();
        //MonoBehaviour.print("COMANDO ACTIVAR");
    }

    public void setUp()
    {
        MonoBehaviour.print("Setup Query Comand: Activate");
    }
}
