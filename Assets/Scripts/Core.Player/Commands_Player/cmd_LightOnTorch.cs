using System;
using Core.Interaction;
using UnityEngine;
using IA.PathFinding;

class cmd_LightOnTorch : BaseQueryCommand
{
    //Animaciones!
    Action<int, bool> setAnimation = delegate { };
    Func<int, bool> getAnimation = delegate { return false; };

    IInteractionComponent InteractionTarget;
    Torch equipedTorch;

    public cmd_LightOnTorch(IInteractionComponent InteractionTarget, Torch equipedTorch, Action<int,bool> setAnimation, Func<int, bool> getAnimation, Transform body, PathFindSolver solver, Func<Node, bool> moveFunction, Action dispose, Action OnChangePath)
        : base(body, solver, moveFunction, dispose, OnChangePath)
    {
        this.setAnimation = setAnimation;
        this.getAnimation = getAnimation;
        this.InteractionTarget = InteractionTarget;
        this.equipedTorch = equipedTorch;
    }

    public override void SetUp()
    {
        var target = InteractionTarget.getInteractionParameters(_body.position);
        _ObjectiveNode = target.safeInteractionNode;

        base.SetUp();
        isReady = true;
    }
    public override void UpdateCommand()
    {
        if (!isInRange(_ObjectiveNode))
        {
            if (!getAnimation(0))
                setAnimation(0, true);

            if (moveFunction(_nextNode) && _solver.currentPath.Count > 0)
                _nextNode = _solver.currentPath.Dequeue();
        }
        else Execute();
    }
    public override void Execute()
    {
        lookTowards(InteractionTarget);
        setAnimation(0, false);
        //setAnimation(1, true);
        InteractionTarget.ExecuteOperation(OperationType.lightOnTorch, new object[] { equipedTorch });
        completed = true;
    }
    public override void Cancel()
    {
        completed = false;
    }
}
