using System;
using Core.Interaction;
using IA.PathFinding;

public class cmd_TrowRock : IQueryComand
{
    Action TriggerAnimation = delegate { };

    IInteractionComponent CommandTarget = null;
    Node TargetNode = null;

    bool IsNode = false;

    public bool completed { get; private set; } = false;
    public bool isReady { get; private set; } = false;
    public bool cashed => true;

    public cmd_TrowRock(IInteractionComponent CommandTarget, Action TriggerAnimation,bool IsNode, Node targetNode)
    {
        this.IsNode = IsNode;
        this.CommandTarget = CommandTarget;
        this.TargetNode = targetNode;
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
        if (IsNode)
        {
            //Si es un nodo/transform.
        }
        else
            CommandTarget.ExecuteOperation(OperationType.Throw);
        completed = true;
    }
    public void Cancel()
    {
        CommandTarget.CancelOperation(OperationType.Throw);
    }
}
