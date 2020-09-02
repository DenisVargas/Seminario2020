using UnityEngine;

namespace Core.Interaction
{
    //Los componentes deberían desactivar su interacción.
    public interface IInteractionComponent
    {
        OperationType OperationType { get; }
        Vector3 LookToDirection { get; }

        Transform transform { get; }
        T GetComponent<T>();

        Vector3 requestSafeInteractionPosition(Vector3 requesterPosition);
        void InputConfirmed(params object[] optionalParams);
        void ExecuteOperation(params object[] optionalParams);
        void CancelOperation(params object[] optionalParams);
    }
}
