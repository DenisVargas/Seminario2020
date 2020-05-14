using System.Collections.Generic;
using UnityEngine;

public class SlimeCoveredObject : MonoBehaviour, IInteractable
{
    List<OperationOptions> suportedOperations = new List<OperationOptions>();

    [SerializeField] float _safeInteractionDistance = 5;

    public Vector3 position { get => transform.position; }
    public Vector3 LookToDirection { get => transform.forward; }

    public void Operate(OperationOptions operation, params object[] optionalParams)
    {
        if (operation == OperationOptions.Ignite)
        {
            Destroy(gameObject);
        }
    }

    public Vector3 requestSafeInteractionPosition(IInteractor requester)
    {
        return ((requester.position - transform.position).normalized *_safeInteractionDistance);
    }

    List<OperationOptions> IInteractable.GetSuportedOperations()
    {
        return suportedOperations;
    }
}
