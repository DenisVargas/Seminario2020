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
    OperationType operation = OperationType.Activate;
    int _animatorParameter = 0;
    int execution = 0;

    public Cmd_Activate(IInteractionComponent CommandTarget, Transform body, PathFindSolver solver, Func<int, bool> getAnimation, Action<int, bool> setAnimation, Func<Node, bool> moveFunction, Action dispose, Action OnChangePath) :
        base(body, solver, moveFunction, dispose, OnChangePath)
    {
        this.CommandTarget = CommandTarget;
        this.setAnimation = setAnimation;
    }

    public override void SetUp()
    {
        var intPar = CommandTarget.getInteractionParameters(_body.position);
        _ObjectiveNode = intPar.safeInteractionNode;
        _animatorParameter = intPar.AnimatorParameter;
        cashed = false;

        needsPremovement = CalculatePremovement(_ObjectiveNode);
        isReady = true;
    }
    public override void UpdateCommand()
    {
        if (completed || execution > 0) return;

        if (!isInRange(_ObjectiveNode))
        {
            //Debug.Log("Estoy Fuera del rango, me muevo.");
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
            //Debug.Log("Llegué al rango.");

            //Ejecuto la animación correspondiente Sobre el Player, confirmo el input al objetivo.
            setAnimation(0, false);
            setAnimation(_animatorParameter, true);
            CommandTarget.InputConfirmed(OperationType.Activate);
            lookTowards(CommandTarget);
            execution++;
        }
    }

    public override void Execute()
    {
        CommandTarget.ExecuteOperation(operation);
        completed = true;
    }

    public override void Cancel()
    {
        CommandTarget.CancelOperation(operation);
    }
}
