using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;
using Core.InventorySystem;
using System;

//Grab podría ser un componente único para objetos consumibles que se activan al momento!

[RequireComponent(typeof(InteractionHandler))]
public class Grab : MonoBehaviour , IInteractionComponent
{
    public bool isDynamic => false;

    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return transform.position;
    }
    public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory, bool ignoreInventory)
    {
        return new List<Tuple<OperationType, IInteractionComponent>>()
        {
            //new Tuple<OperationType, IInteractionComponent>(OperationType.Throw, this)
        };
    }
    public void InputConfirmed(OperationType operation, params object[] optionalParams)
    {
        
    }
    public void ExecuteOperation(OperationType operation, params object[] optionalParams)
    {
        //print("Paso algo relevante para la trama");
        //Transform manito = (Transform)optionalParams[0];
        //transform.SetParent(manito);
        //transform.localPosition = Vector3.zero;
    }
    public void CancelOperation(OperationType operation, params object[] optionalParams)
    {
        
    }

    Vector3 firstPosition;
    public void Throwed(Transform target)
    {
        //uso para tirar
        transform.SetParent(null);
        firstPosition = transform.position;
        StartCoroutine(ParabolicMove(target));
    }
    IEnumerator ParabolicMove(Transform target)
    {
        for (float i = 0; i < 1; i += 0.1f)
        {
            yield return new WaitForSeconds(0.01f);
            //transform.position = Vector3.Slerp(firstPosition, target.transform.position, i);
            transform.position = new Vector3(Mathf.Lerp(firstPosition.x, target.transform.position.x, i), Mathf.Lerp(firstPosition.y, target.position.y, i) + Mathf.Sin(i * Mathf.PI) * 5, Mathf.Lerp(firstPosition.z, target.transform.position.z, i));
        }
    }

    public InteractionParameters getInteractionParameters(Vector3 requesterPosition)
    {
        var graph = FindObjectOfType<NodeGraphBuilder>();
        var node = PathFindSolver.getCloserNodeInGraph(transform.position, graph);
        Vector3 lookAtDir = (transform.position - node.transform.position).normalized;

        return new InteractionParameters(node, lookAtDir);
    }
}
