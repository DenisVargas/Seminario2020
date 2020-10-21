using UnityEngine;
using System.Collections;
using Core.InventorySystem;
using System;
using System.Collections.Generic;

namespace Core.Interaction
{
    //Este es un componente que podemos agregarle a un objeto para que sea inspeccionable.
    //No es un item.
    [RequireComponent(typeof(InteractionHandler))]
    public class Inspeccionable : MonoBehaviour, IInteractionComponent
    {
        [SerializeField] InspectionMenu _displayMenu = null;
        [SerializeField] string IDName = "";
        [SerializeField, TextArea]
        string[] description = { "An irrelevant Object." };

        public bool isDynamic => false;

        private void Awake()
        {
            if (_displayMenu == null)
                _displayMenu = FindObjectOfType<InspectionMenu>();
        }

        public InteractionParameters getInteractionParameters(Vector3 requesterPosition)
        {
            var graph = FindObjectOfType<NodeGraphBuilder>();
            var pickNode = PathFindSolver.getCloserNodeInGraph(transform.position, graph);
            Vector3 LookToDirection = (transform.position - pickNode.transform.position).normalized.YComponent(0);

            return new InteractionParameters(pickNode, LookToDirection);
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
