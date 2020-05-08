using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeCoveredObject : MonoBehaviour , IInteractable
{
    public List<OperationOptions> suportedOperations = new List<OperationOptions>();

    public Vector3 position { get => transform.position; }



    public void Operate(OperationOptions operation, params object[] optionalParams)
    {
        if (operation == OperationOptions.Ignite)
        {
            Destroy(gameObject);
        }
    }

    List<OperationOptions> IInteractable.GetSuportedOperations()
    {
        return suportedOperations;
    }
}
