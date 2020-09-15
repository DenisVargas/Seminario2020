using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;

[RequireComponent(typeof(InteractionHandler))]
public class TrowRockAt : MonoBehaviour, IStaticInteractionComponent
{
    public OperationType OperationType => OperationType.Throw;
    public Vector3 LookToDirection => transform.position;

    BaseNPC npc = null;

    private void Awake()
    {
        npc = GetComponent<BaseNPC>();
    }

    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return npc.transform.position;
    }
    public void InputConfirmed(params object[] optionalParams)
    {
        print("Input Confirmado");
    }
    public void ExecuteOperation(params object[] optionalParams)
    {
        npc.OnHitWithRock(optionalParams);
    }
    public void CancelOperation(params object[] optionalParams)
    {
        print("Input Cancelado");
    }
}
