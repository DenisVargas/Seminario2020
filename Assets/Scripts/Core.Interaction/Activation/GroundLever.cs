using UnityEngine;
using UnityEngine.Events;
using Core.Interaction;
using Core.InventorySystem;
using System;
using System.Collections.Generic;
using IA.PathFinding;

[RequireComponent(typeof(BoxCollider), typeof(InteractionHandler))]
public class GroundLever : MonoBehaviour, IInteractionComponent
{
    [SerializeField] UnityEvent OnActivate = new UnityEvent();
    [SerializeField] UnityEvent OnDeActivate = new UnityEvent();

    [SerializeField] Animator _anims = null;
    [SerializeField] Node _activationPosition = null;
    [SerializeField] Transform _referenceTransform = null;

    Collider _col = null;

    public bool isDynamic => false;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        if (!_col.isTrigger)
            _col.isTrigger = true;
    }
    public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory, bool ignoreInventory = true)
    {
        return new List<Tuple<OperationType, IInteractionComponent>>()
        {
            new Tuple<OperationType, IInteractionComponent>(OperationType.Activate, this)
        };
    }
    public void InputConfirmed(OperationType operation, params object[] optionalParams)
    {
        print("Input Confirmado:: le doy play a la pinche Animación.");
        _anims.SetBool("activated", true);
        OnActivate.Invoke();
    }
    public void ExecuteOperation(OperationType operation, params object[] optionalParams) { }
    public void CancelOperation(OperationType operation, params object[] optionalParams) { }

    public InteractionParameters getInteractionParameters(Vector3 requesterPosition)
    {
        NodeGraphBuilder graph = FindObjectOfType<NodeGraphBuilder>();

        Node SafePosition = PathFindSolver.getCloserNodeInGraph(_activationPosition.transform.position, graph);

        var iparams = new InteractionParameters(SafePosition, _referenceTransform.forward);
        iparams.AnimatorParameter = 2;
        return iparams;
    }

    void OnLeverPulledCompleted_AnimEvent()
    {
        print("OnLeverPullCompleted");
        _anims.SetBool("activated", false);
    }
}
