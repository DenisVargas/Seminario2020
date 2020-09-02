using System;
using Core.Interaction;

public class cmd_TrowRock : IQueryComand
{
    Action TriggerAnimation = delegate { };

    IInteractionComponent CommandTarget;

    public bool completed { get; private set; } = false;
    public bool isReady { get; private set; } = false;
    public bool cashed => true;

    public cmd_TrowRock(IInteractionComponent CommandTarget, Action TriggerAnimation)
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
        //Ejecuto el comando.
        CommandTarget.ExecuteOperation();
        completed = true;
    }
    public void Cancel()
    {
        CommandTarget.CancelOperation();
    }
}
