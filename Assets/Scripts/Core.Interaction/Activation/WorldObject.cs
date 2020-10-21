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

    public InteractionParameters getInteractionParameters(Vector3 requesterPosition)
    {
        NodeGraphBuilder graph = FindObjectOfType<NodeGraphBuilder>();

        IA.PathFinding.Node SafePosition = PathFindSolver.getCloserNodeInGraph(transform.position, graph);
        Vector3 directionToMe = transform.forward;
        //Vector3 directionToMe = (transform.position - SafePosition.transform.position).normalized.YComponent(0);

        return new InteractionParameters(SafePosition, directionToMe);
    }
    public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory, bool ignoreInventory)
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
