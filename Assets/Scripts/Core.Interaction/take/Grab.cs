using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;

[RequireComponent(typeof(InteractionHandler))]
public class Grab : MonoBehaviour , IInteractionComponent
{
    public OperationType OperationType => OperationType.Take;
    public Vector3 LookToDirection => transform.position;

    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return transform.position;
    }
    public void ExecuteOperation(params object[] optionalParams)
    {
        print("Paso algo relevante para la trama");
        Transform manito = (Transform)optionalParams[0];
        transform.SetParent(manito);
        transform.localPosition = Vector3.zero;
    }
    public void InputConfirmed(params object[] optionalParams)
    {
        
    }
    public void CancelOperation(params object[] optionalParams)
    {

    }
}
