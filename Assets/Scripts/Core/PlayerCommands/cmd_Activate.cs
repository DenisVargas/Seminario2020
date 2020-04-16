using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cmd_Activate : IQueryComand
{
    OperationOptions operation;
    IInteractable target;

    public bool completed { get; private set; } = false;

    public cmd_Activate(OperationOptions operation, IInteractable target)
    {
        this.operation = operation;
        this.target = target;
    }

    public void Update()
    {
        //Ejecuto el comando.
        target.Operate((int)operation);
    }

    public void setUp()
    {
        MonoBehaviour.print("Setup Query Comand: Activate");
    }
}
