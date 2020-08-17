using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.PathFinding.Utility;

namespace IA.PathFinding
{
    public static class BreadthFirstSearch<T>
    {
        public static IEnumerable<T> getPath
        (
            T Start,
            Func<T, bool> isTarget,
            Func<T, IEnumerable<T>> getConnections
        )
        {
            Queue<T> open = new Queue<T>();
            HashSet<T> closed = new HashSet<T>();
            Dictionary<T, T> parents = new Dictionary<T, T>();

            open.Enqueue(Start);

            while(open.Any())
            {
                T current = open.Dequeue();
                closed.Add(current);

                if (isTarget(current))
                    parents.getCorrectPath(current);

                foreach (var child in getConnections(current).Where(conectedNode => !closed.Contains(conectedNode)))
                {
                    parents.Add(child, current);
                    closed.Add(child);
                    open.Enqueue(child);
                }
            }

            return Enumerable.Empty<T>();
        }
    }
}
