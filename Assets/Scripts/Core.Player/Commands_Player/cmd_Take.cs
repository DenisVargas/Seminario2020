using Core.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Core.InventorySystem;
using IA.PathFinding;

public class cmd_Take : BaseQueryCommand
{
    Action<int, bool> setAnimation = delegate { };
    Func<int, bool> getAnimation = delegate { return false; };

    Action<Item> equipItem = delegate { };

    Item target;
    InteractionParameters par;

    public cmd_Take(Item target, Action<Item> equipItem, Action<int, bool> setAnimation, Func<int, bool> getAnimation, Transform body, PathFindSolver solver, Func<Node, bool> moveFunction, Action dispose, Action OnChangePath)
        : base(body, solver, moveFunction, dispose, OnChangePath)
    {
        this.setAnimation = setAnimation;
        this.getAnimation = getAnimation;
        this.equipItem = equipItem;
        this.target = target;
    }

    public override void SetUp()
    {
        par = target.getInteractionParameters(_body.position);
        needsPremovement = CalculatePremovement(par.safeInteractionNode);
        isReady = true;
    }
    public override void UpdateCommand()
    {
        if (!isInRange(par.safeInteractionNode))
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
        target.ExecuteOperation(OperationType.Take);
        equipItem(target);
        completed = true;
    }
    public override void Cancel() { }
}

