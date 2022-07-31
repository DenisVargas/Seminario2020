using System;
using IA.PathFinding;
using UnityEngine;
using Core.Interaction;

public class cmd_Inspect : BaseQueryCommand
{
    Action<bool> _setAnim = delegate { };
    Func<bool> getAnim = delegate { return false; };
    Action<string[], Action> _openInspectionMenu = delegate { };
    IInteractionComponent _target = null;

    public cmd_Inspect(IInteractionComponent component, Transform body, PathFindSolver solver, Func<Node, bool> moveFunction, Func<bool> getAnim, Action<bool> setAnim, Action dispose, Action OnChangePath, Action<string[],Action> OpenInspectionMenu) 
        : base(body, solver, moveFunction, dispose, OnChangePath)
    {
        _target = component;
        _openInspectionMenu = OpenInspectionMenu;
        _setAnim = setAnim;
    }

    public override void SetUp()
    {
        var intPar = _target.getInteractionParameters(_body.position);
        _ObjectiveNode = intPar.safeInteractionNode;
        cashed = false;

        needsPremovement = CalculatePremovement(_ObjectiveNode);
        isReady = true;
    }

    public override void UpdateCommand()
    {
        if (!isInRange(_ObjectiveNode))
        {
            if (!getAnim())
                _setAnim(true);

            if (moveFunction(_nextNode) && _solver.currentPath.Count > 0)
                _nextNode = _solver.currentPath.Dequeue();
        }
        else Execute();
    }

    public override void Execute()
    {
        _target.ExecuteOperation(OperationType.inspect);
        lookTowards(_target);
        _setAnim(false);
        dispose();
    }
}
