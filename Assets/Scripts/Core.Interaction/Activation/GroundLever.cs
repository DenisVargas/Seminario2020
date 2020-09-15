using UnityEngine;
using UnityEngine.Events;
using Core.Interaction;

[RequireComponent(typeof(BoxCollider))]
public class GroundLever : MonoBehaviour, IStaticInteractionComponent
{
    [SerializeField] UnityEvent OnActivate = new UnityEvent();
    [SerializeField] UnityEvent OnDeActivate = new UnityEvent();

    [SerializeField] Transform _activationPosition = null;
    [SerializeField] Animator _anims = null;

    Collider _col = null;

    public Vector3 LookToDirection => _activationPosition.forward;
    public bool IsCurrentlyInteractable { get; private set; } = (false);
    public OperationType OperationType => OperationType.Activate;

    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return _activationPosition.position;
    }
    public void InputConfirmed(params object[] optionalParams) { }
    public void ExecuteOperation(params object[] optionalParams)
    {
        _anims.SetBool("Pressed", true);
        OnActivate.Invoke();
    }
    public void CancelOperation(params object[] optionalParams) { }

    private void Awake()
    {
        _col = GetComponent<Collider>();
        if (!_col.isTrigger)
            _col.isTrigger = true;
    }
}
