using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA.PathFinding
{
    public enum NavigationArea
    {
        nonNavegable = 0,
        Navegable
    }

    public class Node : MonoBehaviour
    {
        public int ID = 0;      //ID del nodo dentro de una Graph.
        public List<Node> Connections; //Referencias a los nodos a los que estoy conectado, usado por PathFinding.
        public int area = 0; //Esto lo vamos a usar para detectar si es navegable o no.

    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            //Esto es horrible por si las dudas!
            if (Connections == null) return;

            foreach (var node in Connections)
            {
                if (node.Connections.Contains(this))
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(this.transform.position, node.transform.position);
                }
            }
        }
    #endif
    }
}

