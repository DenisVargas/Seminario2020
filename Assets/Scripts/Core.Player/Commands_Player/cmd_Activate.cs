using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;

public class cmd_Activate : IQueryComand
{
    Action TriggerAnimation = delegate { };
    IStaticInteractionComponent CommandTarget;

    public bool completed { get; private set; } = false;
    public bool cashed => true;
    public bool isReady { get; private set; } = false;

    public cmd_Activate(IStaticInteractionComponent CommandTarget, Action TriggerAnimation)
    {
        this.CommandTarget = CommandTarget;
        this.TriggerAnimation = TriggerAnimation;
    }

    public void SetUp()
    {
        TriggerAnimation();
        isReady = true;
    }
    public void Execute()
    {
        //Ejecuto el comando en el target.
        CommandTarget.ExecuteOperation();
        completed = true;
    }
    public void Cancel()
    {
        CommandTarget.CancelOperation();
    }
}
