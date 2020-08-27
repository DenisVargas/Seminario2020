using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;

[RequireComponent(typeof(Collider), typeof(InteractionHandler))]
public class SlimeCoveredObject : MonoBehaviour, IIgnitableObject
{
    [SerializeField] float _safeInteractionDistance = 5;
    [SerializeField] GameObject[] burnParticles = new GameObject[4];
    [SerializeField] float _fallSpeed = 2;

    Collider _col;
    private bool fall = false;

    private void Awake()
    {
        _col = GetComponent<Collider>();
    }
    #region DEBUG
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _safeInteractionDistance);
    }
#endif
    #endregion
    private void Update()
    {
        if (fall)
        {
            transform.position += Vector3.down * _fallSpeed * Time.deltaTime;
        }
    }

    public OperationType OperationType => OperationType.Ignite;
    public bool lockInteraction { get; private set; } = (false);
    public bool isFreezed { get; set; } = (false);
    public bool Burning { get; private set; } = (false);
    public bool IsActive => gameObject.activeSelf;
    public Vector3 LookToDirection { get => transform.forward; }

    public void StainObjectWithSlime() { }

    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return (transform.position + ((requesterPosition - transform.position).normalized *_safeInteractionDistance));
    }
    public void StartChainReaction() { }
    public void InputConfirmed(params object[] optionalParams) { }
    public void ExecuteOperation(params object[] optionalParams)
    {
        StartCoroutine(Burn());
    }
    public void CancelOperation(params object[] optionalParams) { }

    IEnumerator Burn()
    {
        _col.enabled = false; //Para que no pueda seguir siendo interactuable.
        StartChainReaction();
        yield return new WaitForSeconds(0.8f);
        burnParticles[0].SetActive(true);
        yield return new WaitForSeconds(0.8f);
        burnParticles[1].SetActive(true);
        yield return new WaitForSeconds(0.5f);
        burnParticles[2].SetActive(true);
        yield return new WaitForSeconds(1.2f);
        burnParticles[3].SetActive(true);
        yield return new WaitForSeconds(0.8f);
        burnParticles[4].SetActive(true);
        yield return new WaitForSeconds(1.3f);
        fall = true;
        yield return new WaitForSeconds(4f);
        Destroy(gameObject);
    }

}
