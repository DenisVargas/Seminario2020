using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Core.Interaction;

[RequireComponent(typeof(InteractionHandler))]
public class Activator : MonoBehaviour, IInteractionComponent
{
    [SerializeField] UnityEvent OnActivate = new UnityEvent();

    public Transform ActivationPosition;
    public Vector3 position { get => ActivationPosition.position; }
    public Vector3 LookToDirection { get => ActivationPosition.forward; }

    public OperationType OperationType => OperationType.Activate;
    public bool IsCurrentlyInteractable { get; private set; } = (true);
    public bool InteractionEnabled { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return ActivationPosition.position;
    }
    public void InputConfirmed(params object[] optionalParams) { }
    public void ExecuteOperation(params object[] optionalParams)
    {
        OnActivate.Invoke();
    }
    public void CancelOperation(params object[] optionalParams) { }
}
