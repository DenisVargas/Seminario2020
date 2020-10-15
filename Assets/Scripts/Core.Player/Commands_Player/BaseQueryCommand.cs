using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IA.PathFinding;
using UnityEngine;
using Core.Interaction;

/*
 * Implementa la lógica de movimiento de acuerdo a la distancia, caso contrario ejecuta el contenido real.
 */
public abstract class BaseQueryCommand : IQueryComand
{
    public Func<Node, bool> moveFunction = delegate { return false; };
    public Action dispose = delegate { };
    public Action OnChangePath = delegate { };

    protected Transform _body;
    protected PathFindSolver _solver = null;
    protected Node _currentNode = null; //El nodo en el que estamos parados.
    protected Node _nextNode = null; //El siguiente nodo al que nos movemos.
    protected Node _ObjectiveNode = null; //El objetivo final.

    public BaseQueryCommand(Transform body, PathFindSolver solver, Func<Node, bool> moveFunction, Action dispose, Action OnChangePath)
    {
        this.moveFunction = moveFunction;
        this.dispose = dispose;
        this.OnChangePath = OnChangePath;
        _solver = solver;
        _body = body;
    }

    public bool completed { get; protected set; } = false;
    public bool isReady { get; protected set; } = false;
    public bool needsPremovement { get; protected set; } = false;
    public bool cashed { get; protected set; } = false;

    public virtual void SetUp()
    {
        needsPremovement = CalculatePremovement(_ObjectiveNode);
    }
    public virtual void UpdateCommand() { }
    public virtual void Execute() { }
    public virtual void Cancel() { }

    protected bool isInRange(Node targetNode)
    {
        return Vector3.Distance(_body.position, targetNode.transform.position) < _solver.ProximityTreshold;
    }
    protected bool isInRange(IInteractionComponent targetNode)
    {
        var intParams = targetNode.getInteractionParameters(_body.position);
        float distance = Vector3.Distance(_body.position, intParams.safeInteractionNode.transform.position);
        return distance < _solver.ProximityTreshold;
    }
    /// <summary>
    /// Chequea si la precondicion de posición esta lograda.
    /// </summary>
    /// <param name="node">El nodo en donde sucede la interacción.</param>
    /// <returns>Verdadero si el camino fue encontrado exitosamente.</returns>
    protected bool CalculatePremovement(Node node)
    {
        if (!isInRange(node)) //Si no estamos en rango
        {
            //Move tiene que arrancar calculando el path necesario para recorrer.
            _currentNode = _solver.getCloserNode(_body.position);
            _solver.SetOrigin(_currentNode)
                   .SetTarget(node)
                   .CalculatePathUsingSettings();

            if (_solver.currentPath == null || _solver.currentPath.Count == 0)
            {
                dispose();
                return false;
            }

            _currentNode = _solver.currentPath.Dequeue();
            _nextNode = _solver.currentPath.Dequeue();

            OnChangePath();
            return true;
        }

        return false;
    }
    protected void lookTowards(Node target)
    {
        Vector3 dir = (target.transform.position - _body.transform.position).normalized;
        _body.forward = dir.YComponent(0);
    }
    protected void lookTowards(IInteractionComponent target)
    {
        var pr = target.getInteractionParameters(_body.position);
        _body.forward = pr.orientation.YComponent(0);
    }
}
