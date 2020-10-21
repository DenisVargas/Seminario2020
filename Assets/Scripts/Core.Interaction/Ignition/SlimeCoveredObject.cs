using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;
using Core.InventorySystem;
using System;

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

    public bool isDynamic => false;
    public void StainObjectWithSlime() { }

    IEnumerator Burn()
    {
        _col.enabled = false; //Para que no pueda seguir siendo interactuable.
        //StartChainReaction();
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

    public InteractionParameters getInteractionParameters(Vector3 requesterPosition)
    {
        var graph = FindObjectOfType<NodeGraphBuilder>();

        Vector3 safeInteractionPosition = (transform.position + ((requesterPosition - transform.position).normalized * _safeInteractionDistance));
        var node = PathFindSolver.getCloserNodeInGraph(safeInteractionPosition, graph);
        Vector3 lookAtDir = (transform.position - node.transform.position).normalized;

        return new InteractionParameters(node, lookAtDir);
    }
    public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory, bool ignoreInventory)
    {
        return new List<Tuple<OperationType, IInteractionComponent>>()
        {
            new Tuple<OperationType, IInteractionComponent>(OperationType.Ignite, this)
        };
    }
    public void InputConfirmed(OperationType operation, params object[] optionalParams) { }
    public void ExecuteOperation(OperationType operation, params object[] optionalParams)
    {
        StartCoroutine(Burn());
    }
    public void CancelOperation(OperationType operation, params object[] optionalParams) { }
}
