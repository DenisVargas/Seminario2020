using UnityEngine;
using System.Collections;
using Core.InventorySystem;

namespace Core.Interaction
{
    //Este es un componente que podemos agregarle a un objeto para que sea inspeccionable.
    //No es un item.
    [RequireComponent(typeof(InteractionHandler))]
    public class Inspeccionable : MonoBehaviour, IStaticInteractionComponent
    {
        [SerializeField] string IDName = "";
        [SerializeField, TextArea]
        string description = "An irrelevant Object.";

        public OperationType OperationType => OperationType.inspect;
        public Vector3 LookToDirection => transform.position;
        public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
        {
            return transform.position;
        }
        public void InputConfirmed(params object[] optionalParams) { }
        public void CancelOperation(params object[] optionalParams) { }
        public void ExecuteOperation(params object[] optionalParams)
        {
            //Activo el canvas, y muestro la descripción del item.
        }
    }
}
