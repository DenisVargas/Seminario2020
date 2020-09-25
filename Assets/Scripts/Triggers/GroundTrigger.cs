using Core.InventorySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class GroundTrigger : MonoBehaviour
{
    [SerializeField] UnityEvent OnActivate = new UnityEvent();
    [SerializeField] UnityEvent OnDeActivate = new UnityEvent();

    [SerializeField] Animator _anims = null;
    [SerializeField] float _deactivationTime = 0f;

    Collider _col = null;
    [SerializeField] List<Collider> OnTop = new List<Collider>();

    private void Awake()
    {
        _col = GetComponent<Collider>();
    }

    public void RemoveColliderFromActivationList(GameObject gameObject)
    {
        var toRemove = gameObject.GetComponent<Collider>();

        if (toRemove != null && OnTop.Contains(toRemove))
        {
            OnTop.Remove(toRemove);
            if (OnTop.Count <= 0 && _anims != null)
            {
                _anims.SetBool("Pressed", false);
                OnDeActivate.Invoke();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            OnTop.Add(other);
            _anims.SetBool("Pressed", true);

            var LiveEntity = other.GetComponent<ILivingEntity>();
            if (LiveEntity != null)
                LiveEntity.SubscribeToLifeCicleDependency(RemoveColliderFromActivationList);

            var item = other.GetComponent<Item>();
            if (item != null)
                item.OnPickDepedency += RemoveColliderFromActivationList;

            OnActivate.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger)
        {
            if (OnTop.Contains(other))
                OnTop.Remove(other);

            var LiveEntity = other.GetComponent<ILivingEntity>();
            if (LiveEntity != null)
                LiveEntity.UnsuscribeToLifeCicleDependency(RemoveColliderFromActivationList);

            var item = other.GetComponent<Item>();
            if (item != null)
                item.OnPickDepedency -= RemoveColliderFromActivationList;

            if (OnTop.Count <= 0 )
            {
                _anims.SetBool("Pressed", false);
                StartCoroutine(releaseActivation());
            }
        }
    }

    IEnumerator releaseActivation()
    {
        yield return new WaitForSeconds(_deactivationTime);
        OnDeActivate.Invoke();
    }
}
