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
        [SerializeField] string IDName = "";
        [SerializeField, TextArea]
        string description = "An irrelevant Object.";

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
                new Tuple<OperationType, IInteractionComponent>(OperationType.inspect, this)
            };
        }
        public void InputConfirmed(OperationType operation, params object[] optionalParams)
        {
            
        }
        public void ExecuteOperation(OperationType operation, params object[] optionalParams)
        {
            //Activo el canvas, y muestro la descripción del item.
        }
        public void CancelOperation(OperationType operation, params object[] optionalParams)
        {
            
        }
    }
}
