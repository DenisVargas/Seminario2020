using UnityEngine;
using System.Collections;
using Core.InventorySystem;
using System;
using System.Collections.Generic;
using IA.PathFinding;
using System.Linq;

namespace Core.Interaction
{
    //Este es un componente que podemos agregarle a un objeto para que sea inspeccionable.
    //No es un item.
    [RequireComponent(typeof(InteractionHandler))]
    public class Inspeccionable : MonoBehaviour, IInteractionComponent
    {
        [SerializeField] InspectionMenu _displayMenu = null;
        [SerializeField, TextArea]
        string[] description = { "An irrelevant Object." };

        [Header("Affected Area")]
        public bool blocksNodesUnderneath = true;
        public Node blockedNode = null;
        public bool UseStablishedInteractionPoints = true;
        public List<Node> interactionPoints = new List<Node>();

        public bool isDynamic => false;

        private void Awake()
        {
            if (_displayMenu == null)
                _displayMenu = FindObjectOfType<InspectionMenu>();
            if(blockedNode != null && blocksNodesUnderneath)
                blockedNode.area = NavigationArea.blocked;
        }

        public InteractionParameters getInteractionParameters(Vector3 requesterPosition)
        {
            if (UseStablishedInteractionPoints && interactionPoints.Count > 0)
            {
                Node closerNodeToPlayer = null;
                float minDistance = float.MaxValue;

                foreach (var nodo in interactionPoints)
                {
                    float delta = Vector3.Distance(nodo.transform.position, requesterPosition);
                    if (delta < minDistance)
                    {
                        minDistance = delta;
                        closerNodeToPlayer = nodo;
                    }
                }

                Vector3 lookDir = (transform.position - closerNodeToPlayer.transform.position).normalized.YComponent(0);
                return new InteractionParameters(closerNodeToPlayer, lookDir);
            }

            var graph = FindObjectOfType<NodeGraphBuilder>();
            Node interactionNode = PathFindSolver.getCloserWalkableNodeInGraph(transform.position, graph);
            Vector3 LookToDirection = (transform.position - interactionNode.transform.position).normalized.YComponent(0);

            return new InteractionParameters(interactionNode, LookToDirection);
        }
        public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory, bool ignoreInventory)
        {
            return new List<Tuple<OperationType, IInteractionComponent>>()
            {
                new Tuple<OperationType, IInteractionComponent>(OperationType.inspect, this)
            };
        }
        public void InputConfirmed(OperationType operation, params object[] optionalParams) { }
        public void ExecuteOperation(OperationType operation, params object[] optionalParams)
        {
            //Activo el canvas, y muestro la descripción del item.
            if (_displayMenu != null)
                _displayMenu.DisplayText(description, UnlockInteraction);

        }
        public void CancelOperation(OperationType operation, params object[] optionalParams) { }

        public void UnlockInteraction()
        {
            Debug.Log("UnlockInteraction Executed");
        }
    }
}
