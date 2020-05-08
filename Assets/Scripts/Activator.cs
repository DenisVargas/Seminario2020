using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour, IInteractable
{

    public List<OperationOptions> suportedOperations = new List<OperationOptions>();
    // Start is called before the first frame update
    public Jail _myJail;
    
    

    public Vector3 position { get => transform.position; }



    public void Operate(OperationOptions operation, params object[] optionalParams)
    {
        Debug.LogWarning(string.Format("{0} se ha activado!", gameObject.name));
        if (operation == OperationOptions.Activate)
        {
            Debug.Log("me active");
            _myJail.Drop();
        }
    }
    private void Awake()
    {


    }


    List<OperationOptions> IInteractable.GetSuportedOperations()
    {
        return suportedOperations;
    }
    

}
