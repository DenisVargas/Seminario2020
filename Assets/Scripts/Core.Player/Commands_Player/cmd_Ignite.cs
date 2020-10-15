using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;
using IA.PathFinding;

public class cmd_Ignite : BaseQueryCommand
{
    Action<int, bool> setAnimation = delegate { };
    Func<int, bool> getAnimation = delegate { return false; };

    IInteractionComponent target = null;
    OperationType operation;

    public cmd_Ignite(IInteractionComponent target, OperationType operation, Action<int, bool> setAnimation, Func<int, bool> getAnimation, Transform body, PathFindSolver solver, Func<Node, bool> moveFunction, Action dispose, Action OnChangePath)
        :base(body, solver, moveFunction, dispose, OnChangePath)
    {
        this.target = target;
        this.operation = operation;
        this.setAnimation = setAnimation;
        this.getAnimation = getAnimation;
    }

    public override void SetUp()
    {
        _ObjectiveNode = target.getInteractionParameters(_body.position).safeInteractionNode;
        base.SetUp();

        isReady = true;
    }
    public override void UpdateCommand()
    {
        if (completed) return;

        if (!isInRange(_ObjectiveNode))
        {
            if (!getAnimation(0))
                setAnimation(0, true);

            if (moveFunction(_nextNode) && _solver.currentPath.Count > 0)
                _nextNode = _solver.currentPath.Dequeue();
        }
        else
        {
            setAnimation(0, false);
            setAnimation(1, true);
        }
    }
    public override void Execute()
    {
        //Ejecuto el commando en el target.
        target.ExecuteOperation(operation);
        completed = true;
    }
    public override void Cancel()
    {
        target.CancelOperation(operation);
    }

}
