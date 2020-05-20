using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class GroundLever : MonoBehaviour, IInteractable
{
    UnityEvent OnActivate;
    UnityEvent OnDeActivate;

    [SerializeField] List<OperationOptions> SuportedOperations = new List<OperationOptions>();
    [SerializeField] Transform _activationPosition = null;
    [SerializeField] Animator _anims = null;

    Collider _col = null;

    public Vector3 position => transform.position;
    public Vector3 LookToDirection => _activationPosition.forward;

    public List<OperationOptions> GetSuportedOperations()
    {
        return SuportedOperations;
    }
    public void Operate(OperationOptions selectedOperation, params object[] optionalParams)
    {
        if (selectedOperation == OperationOptions.Activate)
        {
            _anims.SetBool("Pressed", true);
            OnActivate.Invoke();
        }
    }
    public Vector3 requestSafeInteractionPosition(IInteractor requester)
    {
        return _activationPosition.position;
    }

    private void Awake()
    {
        _col = GetComponent<Collider>();
        if (!_col.isTrigger)
            _col.isTrigger = true;
    }
}
