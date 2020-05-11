using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour, IInteractable
{
    public List<OperationOptions> suportedOperations = new List<OperationOptions>();
    // Start is called before the first frame update
    public Jail _myJail;
    public Transform ActivationPosition;

    public Vector3 position { get => ActivationPosition.position; }
    public Vector3 LookToDirection { get => ActivationPosition.forward; }

    public void Operate(OperationOptions operation, params object[] optionalParams)
    {
        Debug.LogWarning(string.Format("{0} se ha activado!", gameObject.name));
        if (operation == OperationOptions.Activate)
        {
            Debug.Log("me active");
            _myJail.Drop();
        }
    }

    List<OperationOptions> IInteractable.GetSuportedOperations()
    {
        return suportedOperations;
    }
}
