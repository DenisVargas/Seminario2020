using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class GroundLever : MonoBehaviour, IInteractable
{
    [SerializeField] UnityEvent OnActivate = new UnityEvent();
    [SerializeField] UnityEvent OnDeActivate = new UnityEvent();

    [SerializeField] List<OperationType> _suportedOperations = new List<OperationType>();
    [SerializeField] Transform _activationPosition = null;
    [SerializeField] Animator _anims = null;

    Collider _col = null;

    public Vector3 position => transform.position;
    public Vector3 LookToDirection => _activationPosition.forward;

    public InteractionParameters GetSuportedInteractionParameters()
    {
        return new InteractionParameters()
        {
            LimitedDisplay = false,
            SuportedOperations = _suportedOperations
        };
    }

    public void OnCancelOperation(OperationType operation, params object[] optionalParams) { }
    public void OnConfirmInput(OperationType selectedOperation, params object[] optionalParams) { }

    public void OnOperate(OperationType selectedOperation, params object[] optionalParams)
    {
        if (selectedOperation == OperationType.Activate)
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
