using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Activator : MonoBehaviour, IInteractable
{
    UnityEvent OnActivate = new UnityEvent();
    public List<OperationType> _suportedOperations = new List<OperationType>();
    // Start is called before the first frame update
    public Transform ActivationPosition;

    public Vector3 position { get => ActivationPosition.position; }
    public Vector3 LookToDirection { get => ActivationPosition.forward; }

    public void OnCancelOperation(OperationType operation, params object[] optionalParams)
    {
        if (operation == OperationType.Activate) { }
    }
    public void OnConfirmInput(OperationType selectedOperation, params object[] optionalParams) { }

    public void OnOperate(OperationType operation, params object[] optionalParams)
    {
        //Debug.LogWarning(string.Format("{0} se ha activado!", gameObject.name));
        if (operation == OperationType.Activate)
        {
            OnActivate.Invoke();
        }
    }

    public Vector3 requestSafeInteractionPosition(IInteractor requester)
    {
        return ActivationPosition.position;
    }

    InteractionParameters IInteractable.GetSuportedInteractionParameters()
    {
        return new InteractionParameters()
        {
            LimitedDisplay = false,
            SuportedOperations = _suportedOperations
        };
    }
}
