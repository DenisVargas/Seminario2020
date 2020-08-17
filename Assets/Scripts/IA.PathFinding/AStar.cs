using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using IA.PathFinding.Utility;

namespace IA.PathFinding
{
    public static class AStar
    {
        /// <summary>
        /// Búsqueda de camino utilizando el algoritmo AStar
        /// </summary>
        /// <param name="Start">Nodo inicial.</param>
        /// <param name="isTarget">Chequea si un nodo es el objetivo buscado.</param>
        /// <param name="getConnections">Retorna las conecciones válidas de un nodo.</param>
        /// <param name="getHeurístic">Retorna el índice de "cercanía" del nodo al Objetivo.</param>
        /// <returns>Devuelve un camino posible al nodo objetivo. Si no encuentra un camino, retorna una collección vacía.</returns>
        public static IEnumerable<T> getPath<T>
        (
            T Start,
            Func<T, bool> isTarget,
            Func<T, IEnumerable<Tuple<T, float>>> getConnections,
            Func<T, float> getHeurístic
        )
        {
            List<T> open = new List<T>();
            HashSet<T> closed = new HashSet<T>();
            Dictionary<T, T> parents = new Dictionary<T, T>();
            Dictionary<T, float> AcumulatedCost = new Dictionary<T, float>();
            Dictionary<T, float> TotalCosts = new Dictionary<T, float>();

            open.Add(Start);
            AcumulatedCost.Add(Start, 0f);
            TotalCosts.Add(Start, getHeurístic(Start));
            parents.Add(Start, Start);

            while (open.Any())
            {
                T currentNode = open.OrderBy(node => AcumulatedCost[node]).First();
                open.Remove(currentNode);
                closed.Add(currentNode);

                if (isTarget(currentNode))
                    return parents.getCorrectPath(currentNode);

                foreach (var pair in getConnections(currentNode).Where(connectionPair => !closed.Contains(connectionPair.Item1)))
                {
                    float temptativeCost = TotalCosts.ContainsKey(pair.Item1) ? TotalCosts[pair.Item1] : float.MaxValue;
                    float alternativeCost = AcumulatedCost[currentNode] + pair.Item2 + getHeurístic(pair.Item1);

                    if (alternativeCost < temptativeCost)
                    {
                        TotalCosts[pair.Item1] = alternativeCost;

                        if (!AcumulatedCost.ContainsKey(pair.Item1))
                            AcumulatedCost.Add(pair.Item1, AcumulatedCost[pair.Item1] + pair.Item2);
                        else
                            AcumulatedCost[pair.Item1] = AcumulatedCost[pair.Item1] + pair.Item2;

                        parents[pair.Item1] = currentNode;
                        closed.Add(pair.Item1);
                    }
                }
            }

            return Enumerable.Empty<T>();
        }
    }
}
