using UnityEngine;
using Core.Interaction;
using Core.InventorySystem;
using System;
using System.Collections.Generic;

public class WorldObject : MonoBehaviour,   IInteractionComponent
{
    [SerializeField] Material onEnterMat = null;
    [SerializeField] Material onExitMat = null;

    Material _normalMat = null;
    Renderer _renderer = null;

    //public bool IsCurrentlyInteractable { get; private set; } = (true);

    public Vector3 LookToDirection => transform.forward;
    public bool isDynamic => false;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _normalMat = _renderer.material;
    }
    private void OnMouseEnter()
    {
        _renderer.material = onEnterMat;
    }
    private void OnMouseExit()
    {
        _renderer.material = onExitMat;
    }

    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return transform.position;
    }
    public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory)
    {
        List<Tuple<OperationType, IInteractionComponent>> operations = new List<Tuple<OperationType, IInteractionComponent>>();
        operations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.Activate, this));
        return operations;
    }
    public void InputConfirmed(OperationType operation, params object[] optionalParams)
    { Debug.LogWarning(string.Format("{0} se ha activado!", gameObject.name));  }
    public void ExecuteOperation(OperationType operation, params object[] optionalParams) { }
    public void CancelOperation(OperationType operation, params object[] optionalParams) { }
}
