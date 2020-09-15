using UnityEngine;
using Core.Interaction;

[RequireComponent(typeof(InteractionHandler))]
public class Gem : MonoBehaviour, IStaticInteractionComponent
{
    [SerializeField] int SpeedRot                 = 2;
    [SerializeField] Transform ActivationPosition = null;
    [SerializeField] Transform GemaView           = null;

    public OperationType OperationType => OperationType.Activate;
    public bool IsCurrentlyInteractable => isActiveAndEnabled;
    public Vector3 position => transform.position;
    public Vector3 LookToDirection => ActivationPosition.forward;

    // Update is called once per frame
    void Update()
    {
        GemaView.transform.Rotate(new Vector3(0, SpeedRot, 0));
    }

    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return ActivationPosition.position;
    }
    public void InputConfirmed(params object[] optionalParams) { Debug.Log($"{gameObject.name}: input Confirmado."); }
    public void ExecuteOperation(params object[] optionalParams)
    {
        GetComponent<LevelTesting>().Restart();
    }
    public void CancelOperation(params object[] optionalParams) { Debug.Log($"{gameObject.name}: Operación cancelada"); }
}
