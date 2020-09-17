using UnityEngine;
using UnityEngine.Events;
using Core.Interaction;
using Core.InventorySystem;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class GroundLever : MonoBehaviour, IInteractionComponent
{
    [SerializeField] UnityEvent OnActivate = new UnityEvent();
    [SerializeField] UnityEvent OnDeActivate = new UnityEvent();

    [SerializeField] Transform _activationPosition = null;
    [SerializeField] Animator _anims = null;

    Collider _col = null;

    public Vector3 LookToDirection => _activationPosition.forward;
    public bool isDynamic => false;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        if (!_col.isTrigger)
            _col.isTrigger = true;
    }
    public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory)
    {
        return new List<Tuple<OperationType, IInteractionComponent>>()
        {
            new Tuple<OperationType, IInteractionComponent>(OperationType.Activate, this)
        };
    }
    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return _activationPosition.position;
    }
    public void InputConfirmed(OperationType operation, params object[] optionalParams)
    {
        
    }
    public void ExecuteOperation(OperationType operation, params object[] optionalParams)
    {
        _anims.SetBool("Pressed", true);
        OnActivate.Invoke();
    }
    public void CancelOperation(OperationType operation, params object[] optionalParams)
    {
        
    }
}
