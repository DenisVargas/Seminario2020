using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.PathFinding;

public class PathFindSolver : MonoBehaviour
{
    [Header("PathFinding")]
    public LayerMask _pathFindingNodeMask = ~0;
    public float _lookUpRange = 10f;
    public float ProximityTreshold = 0.3f;

    [SerializeField] Node _originNode = null;
    [SerializeField] Node _targetNode = null;

    public Node Origin { get => _originNode; }
    public Node Target { get => _targetNode; }

    public Queue<Node> currentPath = new Queue<Node>();

    #region DEBUG
#if UNITY_EDITOR
    [Header(" ============== Debug ================")]
    [SerializeField] Color Debug_DetectionRangeColor = Color.red;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Debug_DetectionRangeColor;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _lookUpRange);
    }
#endif
    #endregion

    public PathFindSolver SetOrigin(Vector3 OriginNode)
    {
        _originNode = getCloserNode(OriginNode);
        return this;
    }
    public PathFindSolver SetOrigin(Node OriginNode)
    {
        this._originNode = OriginNode;
        return this;
    }
    public PathFindSolver SetTarget(Vector3 destinyPosition)
    {
        _targetNode = getCloserNode(destinyPosition);
        return this;
    }
    public PathFindSolver SetTarget(Node destinyNode)
    {
        _targetNode = destinyNode;
        return this;
    }

    /// <summary>
    /// Calcula una ruta hasta la posición indicada en los settings.
    /// </summary>
    public void CalculatePathUsingSettings()
    {
        if (_originNode == null) return;
        currentPath.Clear();

        List<Node> path = getPathToPosition(_originNode);
        foreach (var node in path)
            currentPath.Enqueue(node);
    }

    /// <summary>
    /// Encuentra el nodo mas cercano a la posicion inicial al que este NPC se encuentra.
    /// </summary>
    /// <returns></returns>
    public Node getCloserNode(Vector3 position)
    {
        var builder = FindObjectOfType<NodeGraphBuilder>();
        var posibleNodes = builder.GetComponentsInChildren<Node>()
                                  .OrderBy(n => Vector3.Distance(position, n.transform.position));

        Node closerNode = posibleNodes.First();
        if (closerNode)
            return closerNode;

        return null;
    }

    /// <summary>
    /// Retorna el camino entre dos nodos, usando los settings establecidos en el Componente.
    /// </summary>
    /// <returns>Un camino posible entre 2 nodos. Null si no hay camino posible.</returns>
    public List<Node> getPathWithSettings()
    {
        return getPathToPosition(_originNode);
    }

    /// <summary>
    /// Retorna el camino mas corto a un nodo en el grafo.
    /// </summary>
    /// <param name="startingPoint">El nodo desde donde empiezo a buscar el punto deseado.</param>
    /// <param name="desiredPositionToGo">Posicion global aproximado al que deseo llegar.</param>
    /// <returns></returns>
    private List<Node> getPathToPosition(Node startingPoint)
    {
        return ThetaStar.getPath(startingPoint, isTarget, getNodeConnections, GetHeurístic, hasValidConnection)
                        .ToList();
    }
    private Queue<Node> getPathToPositionAsQueue(Node StartingPoint)
    {
        return ThetaStar.getPath(StartingPoint, isTarget, getNodeConnections, GetHeurístic, hasValidConnection) as Queue<Node>;
    }

    #region PathFinding
    bool isTarget(Node reference)
    {
        return reference == _targetNode;
    }
    bool hasValidConnection(Node A, Node B)
    {
        return A.Connections.Contains(B) && B.Connections.Contains(A);
    }
    float GetHeurístic(Node reference)
    {
        if (_targetNode != null)
            return Vector3.Distance(reference.transform.position, _targetNode.transform.position);
        else
        {
            Debug.LogWarning("Referencia a \"TargetNode\" no esta seteada");
            return float.MaxValue;
        }
    }
    IEnumerable<Tuple<Node, float>> getNodeConnections(Node reference)
    {
        List<Tuple<Node, float>> NodeConnections = new List<Tuple<Node, float>>();

        foreach (var connection in reference.Connections)
        {

            //Aqui, el segundo elemento de la tupla es el peso/costo, por ahora solo determinado por la distancia.

            //Si el nodo es navegable.
            if (connection.area == NavigationArea.Navegable)
            {
                var tuple = Tuple.Create(connection, Vector3.Distance(reference.transform.position, connection.transform.position));
                NodeConnections.Add(tuple);
            }
        }

        return NodeConnections;
    }
    #endregion
}
