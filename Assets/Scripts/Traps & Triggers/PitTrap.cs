using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

[RequireComponent(typeof(BoxCollider))]
public class PitTrap : MonoBehaviour
{
    [SerializeField] UnityEvent OnActivate = new UnityEvent();
    [SerializeField] UnityEvent OnDeActivate = new UnityEvent();

    [SerializeField] Animator _anims = null;

    BoxCollider _col;

    [SerializeField] List<Collider> OnTop = new List<Collider>();

    private void Awake()
    {
        _col = GetComponent<BoxCollider>();
        _col.isTrigger = true;
    }

    public void OnEnableTrap()
    {
        _anims.SetBool("Activate", true);
        OnActivate.Invoke();

        foreach (var coll in OnTop)
        {
            var agent = coll.GetComponentInParent<NavMeshAgent>();
            var rb = coll.GetComponentInParent<Rigidbody>();

            if (agent != null && rb != null)
            {
                agent.enabled = false;
                rb.useGravity = true;
            }
        }
    }

    public void OnDisableTrap()
    {
        _anims.SetBool("Activate", false);
        OnDeActivate.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        OnTop.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (OnTop.Contains(other))
            OnTop.Remove(other);
    }
}
