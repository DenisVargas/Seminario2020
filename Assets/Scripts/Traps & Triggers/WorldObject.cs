using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour, IInteractable
{
    [SerializeField] Material onEnterMat = null;
    [SerializeField] Material onExitMat = null;

    Material _normalMat = null;
    Renderer _renderer = null;

    public List<OperationType> suportedOperations = new List<OperationType>();

    public Vector3 position => transform.position;
    public Vector3 LookToDirection => transform.forward;

    public void OnOperate(OperationType operation, params object[] optionalParams)
    {
        Debug.LogWarning(string.Format("{0} se ha activado!", gameObject.name));
    }

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _normalMat = _renderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseEnter()
    {
        _renderer.material = onEnterMat;
    }

    private void OnMouseExit()
    {
        _renderer.material = onExitMat;
    }

    InteractionParameters IInteractable.GetSuportedInteractionParameters()
    {
        return new InteractionParameters()
        {
            LimitedDisplay = false,
            SuportedOperations = suportedOperations
        };
    }

    public Vector3 requestSafeInteractionPosition(IInteractor requester)
    {
        return position;
    }

    public void OnConfirmInput(OperationType selectedOperation, params object[] optionalParams) { }
}
