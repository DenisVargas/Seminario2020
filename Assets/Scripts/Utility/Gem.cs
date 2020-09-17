using UnityEngine;
using Core.Interaction;
using Core.InventorySystem;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(InteractionHandler))]
public class Gem : MonoBehaviour, IInteractionComponent
{
    [SerializeField] int SpeedRot                 = 2;
    [SerializeField] Transform ActivationPosition = null;
    [SerializeField] Transform GemaView           = null;

    public bool IsCurrentlyInteractable => isActiveAndEnabled;
    public Vector3 position => transform.position;
    public Vector3 LookToDirection => ActivationPosition.forward;

    public bool isDynamic => false;

    // Update is called once per frame
    void Update()
    {
        GemaView.transform.Rotate(new Vector3(0, SpeedRot, 0));
    }

    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return ActivationPosition.position;
    }
    public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory)
    {
        return new List<Tuple<OperationType, IInteractionComponent>>()
        {
            new Tuple<OperationType, IInteractionComponent>(OperationType.Activate, this)
        };
    }
    public void InputConfirmed(OperationType operation, params object[] optionalParams)
    {
        Debug.Log($"{gameObject.name}: input Confirmado.");
    }
    public void ExecuteOperation(OperationType operation, params object[] optionalParams)
    {
        GetComponent<LevelTesting>().Restart();
    }
    public void CancelOperation(OperationType operation, params object[] optionalParams)
    {
        Debug.Log($"{gameObject.name}: Operación cancelada");
    }
}
