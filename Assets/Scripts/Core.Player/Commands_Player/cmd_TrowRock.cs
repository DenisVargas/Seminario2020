using System;
using Core.Interaction;
using IA.PathFinding;

public class cmd_TrowRock : IQueryComand
{
    Action TriggerAnimation = delegate { };

    IInteractionComponent CommandTarget;

    Node targetNode;

    bool IsNode;

    public bool completed { get; private set; } = false;
    public bool isReady { get; private set; } = false;
    public bool cashed => true;

    public cmd_TrowRock(IInteractionComponent CommandTarget, Action TriggerAnimation,bool nodebool, Node NodeTarget)
    {
        if (!nodebool)
            this.CommandTarget = CommandTarget;
        else
            targetNode = NodeTarget;
        
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
