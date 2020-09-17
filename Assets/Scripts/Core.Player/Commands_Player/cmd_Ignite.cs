using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;

public class cmd_Ignite : IQueryComand
{
    Action TriggerAnimation = delegate { };

    IInteractionComponent target = null;
    OperationType operation;

    public bool completed { get; private set; } = (false);
    public bool isReady { get; private set; } = false;
    public bool cashed => true;

    public cmd_Ignite(IInteractionComponent target, OperationType operation, Action TriggerAnimation)
    {
        this.TriggerAnimation = TriggerAnimation;
        this.target = target;
        this.operation = operation;
    }

    public void SetUp()
    {
        TriggerAnimation();
        isReady = true;
    }
    public void Execute()
    {
        //Ejecuto el commando en el target.
        target.ExecuteOperation(operation);
        completed = true;
    }
    public void Cancel()
    {
        target.CancelOperation(operation);
    }
}
