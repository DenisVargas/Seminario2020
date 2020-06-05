using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour, IInteractable
{

    public int SpeedRot;

    public Transform ActivationPosition;

    public Transform GemaView;

    public List<OperationType> _suportedOperations = new List<OperationType>();
    public bool IsCurrentlyInteractable => throw new System.NotImplementedException();

    public int InteractionsAmmount => throw new System.NotImplementedException();

    public Vector3 position => throw new System.NotImplementedException();

    public Vector3 LookToDirection => ActivationPosition.forward;

    public InteractionParameters GetSuportedInteractionParameters()
    {
       
        return new InteractionParameters()
        {
            ActiveTime = 10,
            LimitedDisplay = false,
            SuportedOperations = _suportedOperations
        };
    }

    public void OnCancelOperation(OperationType operation, params object[] optionalParams)
    {
        if (operation == OperationType.Activate) { }
    }

    public void OnConfirmInput(OperationType selectedOperation, params object[] optionalParams)
    {
        throw new System.NotImplementedException();
    }

    public void OnOperate(OperationType selectedOperation, params object[] optionalParams)
    {
        if (selectedOperation == OperationType.Activate)
        {

            Restart();
            
        }
    }

    public Vector3 requestSafeInteractionPosition(IInteractor requester)
    {
        return ActivationPosition.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GemaView.transform.Rotate(new Vector3(0, SpeedRot, 0));
    }

    public void Restart()
    {
        GetComponent<restartLevel>().Restart();
    }
}
