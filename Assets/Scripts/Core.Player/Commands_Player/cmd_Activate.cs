using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;

public class cmd_Activate : IQueryComand
{
    Action TriggerAnimation = delegate { };
    IInteractionComponent CommandTarget;
    OperationType operation;

    public bool completed { get; private set; } = false;
    public bool cashed => true;
    public bool isReady { get; private set; } = false;

    public cmd_Activate(IInteractionComponent CommandTarget, OperationType operation, Action TriggerAnimation)
    {
        this.CommandTarget = CommandTarget;
        this.TriggerAnimation = TriggerAnimation;
        this.operation = operation;
    }

    public void SetUp()
    {
        TriggerAnimation();
        isReady = true;
    }
    public void Execute()
    {
        //Ejecuto el comando en el target.
        CommandTarget.ExecuteOperation(operation);
        completed = true;
    }
    public void Cancel()
    {
        CommandTarget.CancelOperation(operation);
    }
}
