using System;
using IA.PathFinding;
using UnityEngine;

[System.Serializable]
public class cmd_Move : BaseQueryCommand
{
    Action<bool> setAnimation = delegate { };

    public cmd_Move(Transform body, PathFindSolver solver, Node TargetNode, Func<Node, bool> moveFunction, Action<bool> setAnimation, Action dispose, Action OnChangePath)
        : base( body, solver, moveFunction, dispose, OnChangePath)
    {
        this.setAnimation = setAnimation;
        _ObjectiveNode = TargetNode;
    }

    public override void SetUp()
    {
        base.SetUp();
        setAnimation(true);
        isReady = true;
    }

    public override void UpdateCommand()
    {
        //Move Function Retorna true, cuando la distancia al objetivo despues del movimiento es menor al treshold.
        //Si el move Retorna true, entonces actualizamos nuestro siguiente nodo.
        if (moveFunction(_nextNode) && _solver.currentPath.Count > 0)
        {
            _currentNode = _nextNode;
            _nextNode = _solver.currentPath.Dequeue();
        }

        if (isInRange(_ObjectiveNode))
        {
            setAnimation(false);
            dispose();
        }

        //MonoBehaviour.print($"move comand Executing, status {completed ? "Completed" : "On Going"}");
    }
    public override void Cancel() { }
}
