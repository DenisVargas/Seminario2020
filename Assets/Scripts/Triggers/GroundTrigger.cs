using Core.InventorySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class GroundTrigger : MonoBehaviour
{
    [SerializeField] UnityEvent OnActivate = new UnityEvent();
    [SerializeField] UnityEvent OnDeActivate = new UnityEvent();

    [SerializeField] bool _active = false;
    [SerializeField] bool _invertActivation = false;
    [SerializeField] float _deactivationTime = 0f;

    Collider _col = null;
    [SerializeField] Animator _anims = null;
    [SerializeField] List<Collider> OnTop = new List<Collider>();
    [SerializeField] List<int> ignoreLayers = new List<int>();

    public bool Active
    {
        get => _active;
        set
        {
            _active = value;

            if (_anims != null)
                _anims.SetBool("Pressed", _active);

            bool activation = _invertActivation ? !_active : _active;

            if (activation)
                OnActivate.Invoke();
            else
                OnDeActivate.Invoke();
        }
    }

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
            if (OnTop.Count <= 0 && Active)
                Active = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //print($"{other.gameObject.name} esto lo detecto.");
        if (ignoreLayers.Contains(other.gameObject.layer))
            return;

        if (other.gameObject.CompareTag("Box"))
        {
            if (!OnTop.Contains(other))
                OnTop.Add(other);
            var destroyable = other.GetComponent<destroyable>();
            if (destroyable)
                destroyable.onDestroy += RemoveColliderFromActivationList;
        }

        if (!other.isTrigger)
        {
            //print($"{other.gameObject.name} esto lo detecto, no es trigger.");

            if (!OnTop.Contains(other))
                OnTop.Add(other);
            _anims.SetBool("Pressed", true);

            var LiveEntity = other.GetComponent<ILivingEntity>();
            if (LiveEntity != null)
                LiveEntity.SubscribeToLifeCicleDependency(RemoveColliderFromActivationList);

            var item = other.GetComponent<Item>();
            if (item != null)
                item.OnPickDepedency += RemoveColliderFromActivationList;
        }

        if (OnTop.Count > 0 && !Active)
            Active = true;
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

            var destroyable = other.GetComponent<destroyable>();
            if (destroyable)
                destroyable.onDestroy -= RemoveColliderFromActivationList;
        }

        if (OnTop.Count <= 0)
            StartCoroutine(releaseActivation());
    }

    IEnumerator releaseActivation()
    {
        yield return new WaitForSeconds(_deactivationTime);
        Active = false;
    }
}
