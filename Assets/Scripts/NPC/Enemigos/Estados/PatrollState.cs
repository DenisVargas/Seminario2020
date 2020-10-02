using System;
using System.Collections.Generic;
using UnityEngine;
using IA.FSM;
using IA.PathFinding;
using IA.Waypoints;

[RequireComponent(typeof(PathFindSolver), typeof(NodeWaypoint))]
public class PatrollState : State
{
    public Func<bool> checkForPlayer = delegate { return false; };
    public Func<Node, float, bool> moveToNode = delegate { return false; };
    public Action<Node> OnUpdateCurrentNode = delegate { };

    [SerializeField] float _patrollSpeed   = 5f;
    [SerializeField] float _stopTime       = 1.5f;

#if UNITY_EDITOR
    [Header("Testing")]
    [SerializeField] bool debugMessages = false;
#endif

    bool _stoping            = false;
    bool _waitForPosibleRoute = false;
    int _waypointPositionsMoved = 0;
    float _remainingStopTime = 0.0f;

    NodeWaypoint _patrolPoints = null;
    PathFindSolver _solver     = null;

    Node _nextWayPointNode = null;
    Node _nextNode         = null;
    Node _currentNode      = null;

    List<Node> primaryRoute = new List<Node>();
    List<Node> alternativeRoute = new List<Node>();

    private bool _hasToReevaluatePath = false;
    private bool _useSecondaryRoute = false;

    public override void Begin()
    {
//#if UNITY_EDITOR
//        if (debugMessages)
//        {
//            Debug.Log("=========== Begin ==========");
//        } 
//#endif

        //Obtengo referencias.
        if (_solver == null)
            _solver = GetComponent<PathFindSolver>();
        if (_anims == null)
            _anims = GetComponent<Animator>();
        if (_patrolPoints == null)
            _patrolPoints = GetComponent<NodeWaypoint>();

        //Seteo la animacion.
        _anims.SetBool("Walking", true);
        _remainingStopTime = _stopTime;

        _stoping = false;

        //Asigno los nodos de referencia.
        _currentNode = _solver.getCloserNode(transform.position); //Empezamos y consumimos el primero.
        OnUpdateCurrentNode(_currentNode);
        if (_currentNode == _patrolPoints.points[0])
            _waypointPositionsMoved = 1;
        _nextWayPointNode = _patrolPoints.points[_waypointPositionsMoved];

        CalculatePrimaryRoute(_currentNode, _nextWayPointNode, true);

        //Me aseguro de que el próximo nodo no sea el mismo que el current.
        _nextNode = _solver.currentPath.Dequeue();
        if (_nextNode == _currentNode)
            _nextNode = _solver.currentPath.Dequeue();
    }

    public override void Execute()
    {
        //Esto es para chequear un cambio de estado posible.
        if (checkForPlayer())
        {
            SwitchToState(CommonState.pursue);
            return;
        }
        //Timer que se ejecuta cuando estoy en stoping.
        if (_stoping)
        {
            _remainingStopTime -= Time.deltaTime;

            if (_remainingStopTime <= 0)
            {
                _remainingStopTime = _stopTime;
                _stoping = false;
            }
            return;
        }

        //Comprobación de recalculo de camino.

        //Debug.Log("=========== Excecute==========");
        //Debug.Log($"Current is {_currentNode.area.ToString()}");
        //Debug.Log($"Next is {_nextNode.area.ToString()}");

        if (_hasToReevaluatePath)
        {
            //Chequear si mi nodo objetivo esta bloqueado. Caso 1.
            if (_nextWayPointNode.area == NavigationArea.blocked)
            {
                ReevaluateCaseOne();
            }
            else//Si no, Caso 2
            {
                ReevaluateCaseTwo();
            }
            _hasToReevaluatePath = false;
            return;
        }

        if (_waitForPosibleRoute && _nextNode.area == NavigationArea.blocked)
            return;

        //Comprobación de llegada.
        if (Vector3.Distance(_nextNode.transform.position, transform.position) < _solver.ProximityTreshold) //Si la distancia es menor al treshold.
        {
            _currentNode = _nextNode;
            OnUpdateCurrentNode(_currentNode);

            //Encontrar siguiente nodo.
            if (_currentNode == _nextWayPointNode)//Si llegamos al nodo objetivo.
            {
                WaypointPositionReached();

                //Recalcular el camino.
                CalculatePrimaryRoute(_currentNode, _nextWayPointNode, _nextWayPointNode.area == NavigationArea.blocked);

                //Setear nextNode. Usando PrimaryRoute.
                _nextNode = _solver.currentPath.Dequeue();
                _nextNode = _solver.currentPath.Dequeue();

                //Como llegamos al final seteamos stoping y salimos con un return.
                _stoping = true;
                return;
            }

            //Si no llegue al nodo Waypoint Objetivo. Pongo en la cola el siguiente nodo dentro del camino actual.
            if (_solver.currentPath!= null && _solver.currentPath.Count > 0)
                _nextNode = _solver.currentPath.Dequeue(); //Acá esto siempre tiene que estar completo.
        }

        if (_nextNode != null)
            moveToNode(_nextNode, _patrollSpeed); //Me muevo hacia el objetivo actual.
    }

    public override void End()
    {
        _anims.SetBool("Walking", false);
        ClearPrimaryRoute();
        ClearSecondaryRoute();
    }

    void ClearPrimaryRoute()
    {
        foreach (var node in primaryRoute)
            node.OnAreaWeightChanged -= OnNodeLavelChanged;
        primaryRoute.Clear();
    }
    void ClearSecondaryRoute()
    {
        foreach (var node in alternativeRoute)
            node.OnAreaWeightChanged -= OnNodeLavelChanged;
        alternativeRoute.Clear();
    }

    private void ReevaluateCaseOne()
    {
#if unity_editor
        if (debugMessages)
        {
            print("Caso 1");
        } 
#endif

        //El nodo objetivo está bloqueado.
        //Encuentro el nodo navegable mas cercano al objetivo posible.
        Node closerNodeToTarget = null;
        for (int i = primaryRoute.Count - 1; i >= 0; i--)
        {
            if (primaryRoute[i].area == NavigationArea.Navegable)
            {
                closerNodeToTarget = primaryRoute[i];
                break;
            }
        }

        //Calculo alternativeRoute.
        CalculateSecondaryRoute(_currentNode, closerNodeToTarget);
        _useSecondaryRoute = true;

        //Reasigno los nodos necesarios para que me pueda mover al closerNodeToTarget.
        //Reasigno CurrentNode y nextNode;

        _currentNode = _solver.currentPath.Dequeue();
        _nextNode = _solver.currentPath.Dequeue();

        //Si llego al nodo mas cercano al target, y el camino está bloqueado, espero hasta que se desbloquee. ¿Cómo?
        _waitForPosibleRoute = true;
    }
    private void ReevaluateCaseTwo()
    {
//#if UNITY_EDITOR
//        if (debugMessages)
//        {
//            print("Caso 2");
//        } 
//#endif
        //El objetivo sigue siendo navegable, así que tenemos que recalcular Primary Route.
        //Recalculamos el camino principal, ignorando los nodos bloqueados.
        _useSecondaryRoute = false;
        CalculatePrimaryRoute(_currentNode, _nextWayPointNode, false);
        ClearSecondaryRoute();
        //Reasignamos el camino actual.
        _currentNode = _solver.currentPath.Dequeue();
        _nextNode = _solver.currentPath.Dequeue();
    }

    void WaypointPositionReached()
    {
        _waypointPositionsMoved++; //Nos movimos un nodo de posición.
        if (_waypointPositionsMoved >= _patrolPoints.points.Count)
            _waypointPositionsMoved = 0;
        _nextWayPointNode = _patrolPoints.points[_waypointPositionsMoved]; //Seteamos el siguiente _nextWaypointNode;
    }

    //Calcula un camino posible desde mi posición Actual(Nodo) al siguiente punto del waypoint, ignorando nodos bloqueados.
    void CalculatePrimaryRoute(Node origin, Node Target, bool ignoreBloqued)
    {
        //Preseteos.
        _solver.ignoreBloquedNodes(ignoreBloqued); //Calculo el camino ignorando los nodos bloqueados.

        //Limpio todos los settings anteriores para calcular uno totalmente limpio.
        if (primaryRoute.Count != 0)
            ClearPrimaryRoute();

        //Calculo el camino.
        _solver.SetOrigin(origin)
               .SetTarget(Target)
               .CalculatePathUsingSettings();

        //Guardar el resultado en primaryRoute.
        foreach (var node in _solver.currentPath)
        {
            primaryRoute.Add(node);
            node.OnAreaWeightChanged += OnNodeLavelChanged; //Suscribirme a los eventos de cada nodo.
        }

        //PostSeteos.
        _solver.ignoreBloquedNodes(false);
    }
    void CalculateSecondaryRoute(Node origin, Node temporalTarget)
    {
        if (alternativeRoute.Count != 0)
            ClearSecondaryRoute(); //Limpio la ruta secundaria antes de usar uno nuevo.

        Node closerNodeToTarget = null;
        for (int i = primaryRoute.Count - 1; i >= 0; i--)
        {
            if (primaryRoute[i].area == NavigationArea.Navegable)
            {
                closerNodeToTarget = primaryRoute[i];
                break;
            }
        }

        _solver.SetOrigin(_currentNode)
               .SetTarget(closerNodeToTarget)
               .CalculatePathUsingSettings();
    }

    //Si el nodo cambia de modo, ejecutamos esta función.
    void OnNodeLavelChanged (Node source)
    {
        //Comparar el nodo original a nuestro nodo final de la ruta primaria.
        if (source == _nextWayPointNode)
        {
            if (source.area == NavigationArea.Navegable)
            {
                _waitForPosibleRoute = false;
            }
            if (source.area == NavigationArea.blocked)
            {
                _waitForPosibleRoute = true;
            }
        }

        _hasToReevaluatePath = true;

//#if UNITY_EDITOR
//        if (debugMessages && source.area == NavigationArea.blocked)
//        {
//            print($"Has to reevaluate Path cuz {source.ID} has changed his state to {source.area.ToString()}");
//        } 
//#endif
    }
}
