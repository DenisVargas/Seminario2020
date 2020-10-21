using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;
using Core.InventorySystem;
using System;
using IA.PathFinding;
using System.Linq;

public class BurnPoint : MonoBehaviour, IInteractionComponent
{
    public List<Node> interactionPositions = new List<Node>();
    public bool isDynamic => false;

    public InteractionParameters getInteractionParameters(Vector3 requesterPosition)
    {
        Node closerNode = null;
        if (interactionPositions.Count > 0)
            closerNode = interactionPositions[0];
        //interactionPositions.OrderBy(x => Vector3.Distance(x.transform.position, requesterPosition)).FirstOrDefault(null);


        Vector3 direction = -transform.forward;
        if (closerNode != null)
            direction = (transform.position - closerNode.transform.position).normalized.YComponent(0);

        return new InteractionParameters()
        {
            safeInteractionNode = closerNode,
            orientation = direction
        };
    }
    public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory, bool ignoreInventory)
    {
        List < Tuple < OperationType, IInteractionComponent >> operations = new List<Tuple<OperationType, IInteractionComponent>>();

        if (inventory == null) return operations;

        if (inventory.equiped != null && inventory.equiped.ID == 1)
        {
            Torch torch = (Torch)inventory.equiped;
            if (torch.isBurning == false)
                operations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.lightOnTorch, this));
        }

        return operations;
    }
    public void InputConfirmed(OperationType operation, params object[] optionalParams)
    {
        throw new NotImplementedException();
    }
    public void ExecuteOperation(OperationType operation, params object[] optionalParams)
    {
        if (operation == OperationType.lightOnTorch)
        {
            var antorcha = (Torch)optionalParams[0];
            antorcha.isBurning = true;
        }
    }
    public void CancelOperation(OperationType operation, params object[] optionalParams) {}
}
