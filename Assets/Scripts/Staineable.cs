using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Staineable : MonoBehaviour, IIgnitableObject, IInteractable
{
    Renderer _mainRenderer;
    Material NormalMaterial = null;
    [SerializeField] Material StainedMaterial = null;
    [SerializeField] GameObject ignitionParticle = null;

    [SerializeField] bool isStained = false;
    [SerializeField] List<OperationType> _suportedOperations = new List<OperationType>();
    [SerializeField] Collider _interactionCollider = null;

    [SerializeField] float _safeInteractionDistance = 5f;
    [SerializeField] float _otherFiringDelayTime = 0.8f;

    private void Awake()
    {
        _interactionCollider = GetComponent<Collider>();
        _interactionCollider.isTrigger = true;
        _mainRenderer = GetComponentInChildren<Renderer>();
        NormalMaterial = _mainRenderer.material;
    }

    public void StainWithSlime()
    {
        isStained = true;
        if (!_suportedOperations.Contains(OperationType.Ignite))
            _suportedOperations.Add(OperationType.Ignite);
        IsActive = true;
        _mainRenderer.material = StainedMaterial;
    }

    public void DisableInteraction()
    {
        IsCurrentlyInteractable = false;
    }
    public void ResetEntity()
    {
        
    }

    //======================================== Ingnition System ====================================================
    public bool isFreezed { get; set; } = (false);
    public bool Burning { get; set; } = (false);
    public bool IsActive { get; set; } = (false);

    public Vector3 position => transform.position;
    public Vector3 LookToDirection => -transform.forward;

    public void Ignite(float delayTime)
    {
        if (ignitionParticle != null)
            ignitionParticle.SetActive(true);

        //Prengo fuego a las mierdas.
    }
    public void OnInteractionEvent(IIgnitableObject toIgnore) { }

    //======================================== Interaction System ====================================================

    public bool IsCurrentlyInteractable { get; private set; } = (false);
    public int InteractionsAmmount => _suportedOperations.Count;

    public Vector3 requestSafeInteractionPosition(IInteractor requester)
    {
        return transform.position + (((requester.position - transform.position).normalized) * _safeInteractionDistance);
    }
    public InteractionParameters GetSuportedInteractionParameters()
    {
        return new InteractionParameters()
        {
            LimitedDisplay = false,
            ActiveTime = 5f,
            SuportedOperations = _suportedOperations
        };
    }

    public void OnConfirmInput(OperationType selectedOperation, params object[] optionalParams) { }
    public void OnOperate(OperationType selectedOperation, params object[] optionalParams)
    {
        if (selectedOperation == OperationType.Ignite)
        {
            Ignite(_otherFiringDelayTime);
        }
    }
    public void OnCancelOperation(OperationType operation, params object[] optionalParams) { }

#if UNITY_EDITOR
    //======================================== DEBUG ====================================================================
    [SerializeField] bool DEBUG_safeInteractionDistance = false;
    [SerializeField] Color DEBUG_safeInteractionDistance_Color = Color.black;

    private void OnDrawGizmosSelected()
    {
        if (DEBUG_safeInteractionDistance)
        {
            Gizmos.color = DEBUG_safeInteractionDistance_Color;
            Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
            Gizmos.DrawWireSphere(transform.position, _safeInteractionDistance);
        }
    }
#endif
}
