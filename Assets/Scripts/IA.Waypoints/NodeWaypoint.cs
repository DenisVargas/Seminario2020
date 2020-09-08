using System;
using System.Collections.Generic;
using UnityEngine;
using IA.PathFinding;

namespace IA.Waypoints
{
    [Serializable]
    public class NodeWaypoint: MonoBehaviour
    {
        public int CurrentPositionIndex = 0;
        public List<Node> points = new List<Node>();

        public Vector3 getNextPosition(int currentPosition)
        {
            if (currentPosition >= points.Count)
                currentPosition = 0;

            return points[currentPosition].transform.position;
        }
    }
}
