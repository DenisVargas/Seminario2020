using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SlimeCoveredObject : MonoBehaviour, IInteractable
{
    [SerializeField] List<OperationType> _suportedOperations = new List<OperationType>();
    [SerializeField] float _safeInteractionDistance = 5;
    [SerializeField] GameObject[] burnParticles = new GameObject[4];
    [SerializeField] float _fallSpeed = 2;

    Collider _col;
    private bool fall = false;

    private void Awake()
    {
        _col = GetComponent<Collider>();
    }

    private void Update()
    {
        if (fall)
        {
            transform.position += Vector3.down * _fallSpeed * Time.deltaTime;
        }
    }

    public Vector3 position { get => transform.position; }
    public Vector3 LookToDirection { get => transform.forward; }

    public Vector3 requestSafeInteractionPosition(IInteractor requester)
    {
        return (transform.position + ((requester.position - transform.position).normalized *_safeInteractionDistance));
    }
    public void OnConfirmInput(OperationType selectedOperation, params object[] optionalParams) { }
    public void OnOperate(OperationType operation, params object[] optionalParams)
    {
        if (operation == OperationType.Ignite)
        {
            StartCoroutine(Burn());
        }
    }
    InteractionParameters IInteractable.GetSuportedInteractionParameters()
    {
        return new InteractionParameters()
        {
            LimitedDisplay = false,
            SuportedOperations = _suportedOperations
        };
    }
    public void OnCancelOperation(OperationType operation, params object[] optionalParams) { }

    IEnumerator Burn()
    {
        _col.enabled = false; //Para que no pueda seguir siendo interactuable.
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _safeInteractionDistance);
    }
#endif
}
