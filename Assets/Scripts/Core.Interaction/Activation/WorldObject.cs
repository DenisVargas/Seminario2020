using UnityEngine;
using Core.Interaction;

public class WorldObject : MonoBehaviour, IInteractionComponent
{
    [SerializeField] Material onEnterMat = null;
    [SerializeField] Material onExitMat = null;

    Material _normalMat = null;
    Renderer _renderer = null;

    public OperationType OperationType => OperationType.Activate;
    public Vector3 LookToDirection => transform.forward;
    public bool IsCurrentlyInteractable { get; private set; } = (true);

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _normalMat = _renderer.material;
    }
    private void OnMouseEnter()
    {
        _renderer.material = onEnterMat;
    }
    private void OnMouseExit()
    {
        _renderer.material = onExitMat;
    }

    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return transform.position;
    }
    public void InputConfirmed(params object[] optionalParams) { Debug.LogWarning(string.Format("{0} se ha activado!", gameObject.name));  }
    public void ExecuteOperation(params object[] optionalParams) { }
    public void CancelOperation(params object[] optionalParams) { }
}
