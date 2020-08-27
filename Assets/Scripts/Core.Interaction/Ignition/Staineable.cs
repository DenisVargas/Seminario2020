using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;

[RequireComponent(typeof(Collider))]
public class Staineable : MonoBehaviour//, IIgnitableObject
{
    [SerializeField] OperationType _operationType = OperationType.Ignite;

    Renderer _mainRenderer;
    Material NormalMaterial = null;
    [SerializeField] Material StainedMaterial = null;
    [SerializeField] GameObject ignitionParticle = null;

    //[SerializeField] bool isStained = false;
    [SerializeField] List<OperationType> _suportedOperations = new List<OperationType>();
    [SerializeField] Collider _interactionCollider = null;

    [SerializeField] float _safeInteractionDistance = 5f;
    //[SerializeField] float _otherFiringDelayTime = 0.8f;

    #region DEBUG
    #if UNITY_EDITOR
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
    #endregion

    private void Awake()
    {
        _interactionCollider = GetComponent<Collider>();
        _interactionCollider.isTrigger = true;
        _mainRenderer = GetComponentInChildren<Renderer>();
        NormalMaterial = _mainRenderer.material;
    }

    public void StainWithSlime()
    {
        //isStained = true;
        //if (!_suportedOperations.Contains(OperationType.Ignite))
        //    _suportedOperations.Add(OperationType.Ignite);
        IsActive = true;
        _mainRenderer.material = StainedMaterial;
    }

    public void DisableInteraction()
    {
        //IsCurrentlyInteractable = false;
    }

    //======================================= Ingnition System ====================================================

    public OperationType OperationType => _operationType;
    public bool isFreezed { get; set; } = (false);
    public bool Burning { get; set; } = (false);
    public bool IsActive { get; set; } = (false);
    public bool lockInteraction => false;
    public Vector3 LookToDirection => -transform.forward;

    public Vector3 requestSafeInteractionPosition(IInteractor requester)
    {
        return transform.position + (((requester.position - transform.position).normalized) * _safeInteractionDistance);
    }
    public void InputConfirmed(params object[] optionalParams) { }
    public void Execute(params object[] optionalParams)
    {
        if (ignitionParticle != null)
            ignitionParticle.SetActive(true);

        //Prengo fuego a las mierdas.
    }
    public void CancelOperation(params object[] optionalParams) { }


}
