using UnityEngine;
using Core.Interaction;
using Core.InventorySystem;
using System;
using System.Collections.Generic;
using IA.PathFinding;

[RequireComponent(typeof(InteractionHandler))]
public class Gem : MonoBehaviour, IInteractionComponent
{
    [SerializeField] int SpeedRot        = 2;
    [SerializeField] Node ActivationNode = null;
    [SerializeField] Transform GemaView  = null;

    public bool isDynamic => false;

#if UNITY_EDITOR
    [SerializeField] bool debugThis = false; 
#endif

    // Update is called once per frame
    void Update()
    {
        GemaView.transform.Rotate(new Vector3(0, SpeedRot, 0));
    }

    public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory, bool ignoreInventory)
    {
        return new List<Tuple<OperationType, IInteractionComponent>>()
        {
            new Tuple<OperationType, IInteractionComponent>(OperationType.Activate, this)
        };
    }
    public void InputConfirmed(OperationType operation, params object[] optionalParams)
    {
#if UNITY_EDITOR
        if (debugThis)
            Debug.Log($"{gameObject.name}: input Confirmado."); 
#endif
    }
    public void ExecuteOperation(OperationType operation, params object[] optionalParams)
    {
        if(operation == OperationType.Activate)
        {
            //Core.SaveSystem.Level.RestartCurrentLevel();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
    public void CancelOperation(OperationType operation, params object[] optionalParams)
    {
        Debug.Log($"{gameObject.name}: Operación cancelada");
    }

    public InteractionParameters getInteractionParameters(Vector3 requesterPosition)
    {
        NodeGraphBuilder graph = FindObjectOfType<NodeGraphBuilder>();

        //Estaría bueno tener un par de puntos de referencia que podriamos utilizar como posiciones.
        //Seleccionamos la posicion cuya distancia es menor al requester y luego obtenemos dicho nodos y se lo devolvemos.

        Node SafePosition = ActivationNode;
        Vector3 directionToMe = ( transform.position - SafePosition.transform.position).normalized.YComponent(0);

        return new InteractionParameters(SafePosition, directionToMe, 1);
    }
}
