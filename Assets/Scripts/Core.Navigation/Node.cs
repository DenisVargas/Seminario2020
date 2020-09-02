using System;
using System.Collections.Generic;
using UnityEngine;

namespace IA.PathFinding
{
    //Navigation Area es una etiqueta.
    public enum NavigationArea
    {
        Navegable = 0,
        blocked
    }

    public class Node : MonoBehaviour
    {
        public event Action<NavigationArea> OnAreaWeightChanged = delegate { }; //Esto es un evento de navegación.

        public int ID = 0;      //ID del nodo dentro de una Graph.
        public List<Node> Connections = new List<Node>(); //Referencias a los nodos a los que estoy conectado, usado por PathFinding.
        public NavigationArea area = 0; //Esto lo vamos a usar para detectar si es navegable o no.

        #region DEBUG
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
        #endregion

        public void ChangeNodeState(NavigationArea state)
        {
            area = state;
            OnAreaWeightChanged(area);
        }
    }
}

