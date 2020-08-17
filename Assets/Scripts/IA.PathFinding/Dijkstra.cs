using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IA.PathFinding.Utility;

namespace IA.PathFinding
{
    public static class Dijkstra
    {
        /// <summary>
        /// Búsqueda de camino utilizando el algoritmo de Dijkstra.
        /// </summary>
        /// <param name="Start">Nodo inicial.</param>
        /// <param name="isTarget">Chequea si un nodo es el objetivo buscado.</param>
        /// <param name="getConnections">Retorna las conecciones de un nodo.</param>
        /// <returns>Devuelve un camino posible al nodo objetivo.</returns>
        public static IEnumerable<T> getPath<T>
        (
            T Start,
            Func<T, bool> isTarget,
            Func<T, IEnumerable<Tuple<T, float>>> getConnections
        )
        {
            List<T> open = new List<T>();
            HashSet<T> closed = new HashSet<T>();
            Dictionary<T, T> parents = new Dictionary<T, T>();
            Dictionary<T, float> Costs = new Dictionary<T, float>();

            open.Add(Start);
            Costs.Add(Start, 0);

            while(open.Any())
            {
                T currentNode = open.OrderBy(node => Costs[node]).First();
                open.Remove(currentNode);
                closed.Add(currentNode);

                if (isTarget(currentNode))
                    return parents.getCorrectPath(currentNode);

                foreach (var pair in getConnections(currentNode).Where(connectionPair => !closed.Contains(connectionPair.Item1)))
                {
                    float temptativeCost = Costs.ContainsKey(pair.Item1) ? Costs[pair.Item1] : float.MaxValue;
                    float alternativeCost = Costs[currentNode] + pair.Item2;

                    if (temptativeCost > alternativeCost)
                    {
                        Costs[pair.Item1] = alternativeCost;
                        parents[pair.Item1] = currentNode;
                        open.Add(pair.Item1);
                    }
                }
            }

            return Enumerable.Empty<T>();
        }
    } 
}
