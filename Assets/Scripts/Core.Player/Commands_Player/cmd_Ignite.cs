using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;

public class cmd_Ignite : IQueryComand
{
    Action TriggerAnimation = delegate { };

    IStaticInteractionComponent target;

    public bool completed { get; private set; } = (false);
    public bool isReady { get; private set; } = false;
    public bool cashed => true;

    public cmd_Ignite(IStaticInteractionComponent target, Action TriggerAnimation)
    {
        this.TriggerAnimation = TriggerAnimation;
        this.target = target;
    }

    public void SetUp()
    {
        TriggerAnimation();
        isReady = true;
    }
    public void Execute()
    {
        //Ejecuto el commando en el target.
        target.ExecuteOperation();
        completed = true;
    }
    public void Cancel()
    {
        target.CancelOperation();
    }
}
