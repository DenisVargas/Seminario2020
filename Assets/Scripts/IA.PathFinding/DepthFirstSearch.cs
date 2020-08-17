using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using IA.PathFinding.Utility;

namespace IA.PathFinding
{
    public static class DepthFirstSearch
    {
        /// <summary>
        /// Busqueda de camino utilizando el algoritmo DepthFirstSearch (DFS).
        /// </summary>
        /// <param name="Start">Nodo inicial</param>
        /// <param name="isTarget">Chequea si un nodo es el objetivo buscado.</param>
        /// <param name="getConnections">Retorna las conecciones de un nodo.</param>
        /// <returns>Devuelve un camino posible al nodo objetivo.</returns>
        public static IEnumerable<T> getPath<T>
        (
            T Start,
            Func<T, bool> isTarget,
            Func<T, IEnumerable<T>> getConnections
        )
        {
            Stack<T> open = new Stack<T>();
            HashSet<T> closed = new HashSet<T>();
            Dictionary<T, T> parents = new Dictionary<T, T>();

            open.Push(Start);

            while (open.Any())
            {
                T current = open.Pop();
                closed.Add(current);

                if (isTarget(current))
                    return parents.getCorrectPath(current);

                foreach (var child in getConnections(current).Where(conectedNode => !closed.Contains(conectedNode)))
                {
                    parents.Add(child, current);
                    closed.Add(child);
                    open.Push(child);
                }
            }

            return Enumerable.Empty<T>();
        }
    }
}
