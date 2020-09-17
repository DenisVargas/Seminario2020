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

        public Vector3 LookToDirection => transform.position;
        public bool isDynamic => false;

        public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
        {
            return transform.position;
        }
        public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory)
        {
            return new List<Tuple<OperationType, IInteractionComponent>>()
            {
                new Tuple<OperationType, IInteractionComponent>(OperationType.Exchange, this)
            };
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
