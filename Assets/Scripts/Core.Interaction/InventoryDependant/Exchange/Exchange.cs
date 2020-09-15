using UnityEngine;
using System.Collections;
using Core.InventorySystem;

namespace Core.Interaction
{
    [RequireComponent(typeof(InteractionHandler))]
    public class Exchange: MonoBehaviour, IStaticInteractionComponent
    {
        [SerializeField] ItemData data;

        public OperationType OperationType => OperationType.Exchange;
        public Vector3 LookToDirection => transform.position;
        public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
        {
            return transform.position;
        }

        public void InputConfirmed(params object[] optionalParams) { }
        public void CancelOperation(params object[] optionalParams) { }
        public void ExecuteOperation(params object[] optionalParams)
        {
            //Debo dejar el item equipado en la posición de este objeto.
            //Equipar/Attachear este objeto al player.
        }
    }
}
