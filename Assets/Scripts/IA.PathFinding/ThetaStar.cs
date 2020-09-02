using System;
using System.Linq;
using System.Collections.Generic;
using IA.PathFinding.Utility;

namespace IA.PathFinding
{
    public static class ThetaStar
    {
        /// <summary>
        /// Búsqueda de camino utilizando el algoritmo ThetaStar
        /// </summary>
        /// <param name="Start">Nodo inicial.</param>
        /// <param name="isTarget">Chequea si un nodo el es objetivo buscado.</param>
        /// <param name="getConnections">Retorna las conecciones válidas de un nodo.</param>
        /// <param name="getHeuristic">Calcula el índice de "cercanía" del nodo al Objetivo.</param>
        /// <param name="HasConnection">Chequea si un nodo esta directamente conectado a otro.</param>
        /// <returns>Devuelve un camino posible al nodo objetivo. Si no encuentra nu camino, retorna una collección vacía.</returns>
        public static IEnumerable<NodeType> getPath<NodeType>
        (
            NodeType Start,
            Func<NodeType, bool> isTarget,
            Func<NodeType, IEnumerable<Tuple<NodeType, float>>> getConnections,
            Func<NodeType, float> getHeuristic,
            Func<NodeType, NodeType, bool> HasConnection
        )
        {
            List<NodeType> open = new List<NodeType>();
            HashSet<NodeType> closed = new HashSet<NodeType>();
            Dictionary<NodeType, NodeType> parents = new Dictionary<NodeType, NodeType>();
            Dictionary<NodeType, float> AcumulatedCosts = new Dictionary<NodeType, float>();
            Dictionary<NodeType, float> TotalCosts = new Dictionary<NodeType, float>();

            open.Add(Start);
            AcumulatedCosts.Add(Start, 0f);
            TotalCosts.Add(Start, getHeuristic(Start));
            parents.Add(Start, Start);

            while (open.Any())
            {
                NodeType currentNode = open.OrderBy(node => AcumulatedCosts[node]).First();
                open.Remove(currentNode);
                closed.Add(currentNode);

                if (isTarget(currentNode))
                    return parents.getCorrectPath(currentNode);

                foreach (Tuple<NodeType, float> pair in getConnections(currentNode).Where(connectionPair => !closed.Contains(connectionPair.Item1)))
                {
                    open.Add(pair.Item1);

                    float previousCost = TotalCosts.ContainsKey(pair.Item1) ? TotalCosts[pair.Item1] : float.MaxValue;

                    float alternativeCost = AcumulatedCosts[currentNode] + pair.Item2 + getHeuristic(pair.Item1);

                    NodeType grandParent = parents[currentNode];
                    bool grandParentHasPriority = false;

                    if (!currentNode.Equals(Start) && HasConnection(grandParent, pair.Item1))
                    {
                        float secondaryRouteCost = AcumulatedCosts[grandParent] + pair.Item2 + getHeuristic(pair.Item1);

                        if (secondaryRouteCost < alternativeCost)
                        {
                            grandParentHasPriority = true;
                            alternativeCost = secondaryRouteCost;
                        }
                    }

                    if (alternativeCost < previousCost)
                    {
                        TotalCosts[pair.Item1] = alternativeCost;
                        AcumulatedCosts[pair.Item1] = AcumulatedCosts[currentNode] + pair.Item2;
                        parents[pair.Item1] = grandParentHasPriority ? grandParent : currentNode;
                        closed.Add(pair.Item1);
                    }
                }
            }

            return Enumerable.Empty<NodeType>();
        }
    }
}
