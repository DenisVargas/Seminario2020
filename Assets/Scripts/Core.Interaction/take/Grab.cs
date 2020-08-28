using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;

[RequireComponent(typeof(InteractionHandler))]
public class Grab : MonoBehaviour , IInteractionComponent
{
    
    public OperationType OperationType => OperationType.Take;

    public Vector3 LookToDirection => transform.position;

    public void CancelOperation(params object[] optionalParams)
    {
    }

    public void ExecuteOperation(params object[] optionalParams)
    {
        Transform manito = (Transform)optionalParams[0];
        transform.SetParent(manito);
    }

    public void InputConfirmed(params object[] optionalParams)
    {
        
    }

    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return transform.position;
    }

    
}
