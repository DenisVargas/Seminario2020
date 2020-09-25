using System;
using System.Collections.Generic;
using UnityEngine;
using Core.InventorySystem;

namespace Core.Interaction
{
    public interface IInteractionComponent
    {
        bool isDynamic { get; }
        Transform transform { get; }
        T GetComponent<T>();

        InteractionParameters getInteractionParameters(Vector3 requesterPosition);
        List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory);
        void InputConfirmed(OperationType operation, params object[] optionalParams);
        void ExecuteOperation(OperationType operation, params object[] optionalParams);
        void CancelOperation(OperationType operation, params object[] optionalParams);
    }
}
