using UnityEngine;
using Core.InventorySystem;
using System;
using System.Collections.Generic;

namespace Core.Interaction
{
    [RequireComponent(typeof(InteractionHandler))]
    public class Exchange: MonoBehaviour, IInteractionComponent
    {
        [SerializeField] ItemData data;
        public bool isDynamic => false;

        public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory, bool ignoreInventory)
        {
            return new List<Tuple<OperationType, IInteractionComponent>>()
            {
                new Tuple<OperationType, IInteractionComponent>(OperationType.Exchange, this)
            };
        }

        public InteractionParameters getInteractionParameters(Vector3 requesterPosition)
        {
            var graph = FindObjectOfType<NodeGraphBuilder>();
            var pickNode = PathFindSolver.getCloserNodeInGraph(transform.position, graph);
            Vector3 LookToDirection = (transform.position - pickNode.transform.position).normalized.YComponent(0);

            return new InteractionParameters(pickNode, LookToDirection);
        }
        public void InputConfirmed(OperationType operation, params object[] optionalParams)
        {
            
        }
        public void ExecuteOperation(OperationType operation, params object[] optionalParams)
        {
            //Debo dejar el item equipado en la posición de este objeto.
            //Equipar/Attachear este objeto al player.
        }
        public void CancelOperation(OperationType operation, params object[] optionalParams)
        {
            throw new NotImplementedException();
        }
    }
}
