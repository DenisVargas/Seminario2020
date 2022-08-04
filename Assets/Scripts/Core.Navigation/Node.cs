using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;

namespace IA.PathFinding
{
    //Navigation Area es una etiqueta.
    public enum NavigationArea
    {
        Navegable = 0,
        blocked
    }

    [Serializable]
    public class Node : MonoBehaviour
    {
        public event Action<Node> OnAreaWeightChanged = delegate { }; //Esto es un evento de navegación.

        public int ID = 0;      //ID del nodo dentro de una Graph.
        public List<Node> Connections = new List<Node>(); //Referencias a los nodos a los que estoy conectado, usado por PathFinding.
        public NavigationArea area = 0; //Esto lo vamos a usar para detectar si es navegable o no.
        public IInteractable handler = null;
        public IInteractable EntityInteractor { get; set; } = null; //Esto es una propiedad que nos va a permitir añadir una referencia a un interaction handler.

        public void clearHandler()
        {
            handler = null;
        }

        #region DEBUG
#if UNITY_EDITOR

        public bool debugThisNode = false;
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
#if UNITY_EDITOR
            if (debugThisNode)
                print("Debugging");
#endif

            area = state;
            OnAreaWeightChanged(this);
        }
    }
}

