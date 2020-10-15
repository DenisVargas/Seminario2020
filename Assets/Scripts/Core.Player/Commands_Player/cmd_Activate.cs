using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;
using IA.PathFinding;

public class Cmd_Activate : BaseQueryCommand
{
    Action<int, bool> setAnimation = delegate { };
    Func<int, bool> getAnimation = delegate { return false; };
    IInteractionComponent CommandTarget;
    OperationType operation;

    public Cmd_Activate(IInteractionComponent CommandTarget, OperationType operation, Transform body, PathFindSolver solver, Func<int, bool> getAnimation, Action<int, bool> setAnimation, Func<Node, bool> moveFunction, Action dispose, Action OnChangePath) :
        base(body, solver, moveFunction, dispose, OnChangePath)
    {
        this.CommandTarget = CommandTarget;
        this.setAnimation = setAnimation;
        this.operation = operation;
    }

    public override void SetUp()
    {
        var intPar = CommandTarget.getInteractionParameters(_body.position);
        _ObjectiveNode = intPar.safeInteractionNode;
        cashed = false;

        needsPremovement = CalculatePremovement(_ObjectiveNode);
        isReady = true;
    }
    public override void UpdateCommand()
    {
        if (completed) return;

        if (!isInRange(_ObjectiveNode))
        {
            Debug.Log("Estoy Fuera del rango, me muevo.");
            //Se entiende que el índice 0 es walk, mientras que el índice 1 es PullLever.
            if (!getAnimation(0))
                setAnimation(0, true);

            //Move Function Retorna true, cuando la distancia al objetivo despues del movimiento es menor al treshold.
            //Si el move Retorna true, entonces actualizamos nuestro siguiente nodo.
            if (moveFunction(_nextNode) && _solver.currentPath.Count > 0)
                _nextNode = _solver.currentPath.Dequeue();
        }
        else
        {
            //Ejecuto el comando en el target.
            Debug.Log("Llegué al rango.");
            setAnimation(0, false);
            setAnimation(1, true);
        }
    }

    public override void Execute()
    {
        lookTowards(CommandTarget);
        CommandTarget.ExecuteOperation(operation);
        completed = true;
    }

    public override void Cancel()
    {
        CommandTarget.CancelOperation(operation);
    }
}
