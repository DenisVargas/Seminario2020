using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;
using Core.InventorySystem;
using System;

[RequireComponent(typeof(InteractionHandler))]
public class TrowRockAt : MonoBehaviour, IInteractionComponent
{
    public bool isDynamic => false;

    BaseNPC npc = null;

    private void Awake()
    {
        npc = GetComponent<BaseNPC>();
    }

    public void InputConfirmed(OperationType operation, params object[] optionalParams)
    {
        print("Input Confirmado");
    }
    public void ExecuteOperation(OperationType operation, params object[] optionalParams)
    {
        npc.OnHitWithRock(optionalParams);
    }
    public void CancelOperation(OperationType operation, params object[] optionalParams)
    {
        print("Input Cancelado");
    }
    public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory)
    {
        return new List<Tuple<OperationType, IInteractionComponent>>()
        { new Tuple<OperationType, IInteractionComponent>(OperationType.Throw, this)};
    }

    public InteractionParameters getInteractionParameters(Vector3 requesterPosition)
    {
        var graph = FindObjectOfType<NodeGraphBuilder>();
        var pickNode = PathFindSolver.getCloserNodeInGraph(npc.transform.position, graph);
        Vector3 LookToDirection = (transform.position - pickNode.transform.position).normalized.YComponent(0);

        return new InteractionParameters(pickNode, LookToDirection);
    }
}
