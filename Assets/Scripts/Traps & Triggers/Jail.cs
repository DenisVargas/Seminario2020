using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Jail : MonoBehaviour
{
    Animator _anims;
    [SerializeField] TrapHitBox _hitbox = null;
    [SerializeField] Collider _destructibleHitbox = null;
    [SerializeField] float deactivateDelay = 1f;

    void Awake()
    {
        _anims = GetComponent<Animator>();
    }

    public void Drop()
    {
        _anims.SetBool("Activated", true);
    }

    public void AV_FallEnded()
    {
        if (_hitbox != null)
        {
            _hitbox.IsActive = true;
        }
        if (_destructibleHitbox != null)
        {
            _destructibleHitbox.enabled = true;
        }
        StartCoroutine(delayedDeactivate());
    }

    IEnumerator delayedDeactivate()
    {
        yield return new WaitForSeconds(deactivateDelay);
        _anims.SetBool("Activated", false);
        if (_hitbox != null)
        {
            _hitbox.IsActive = false;
        }
        if (_destructibleHitbox != null)
        {
            _destructibleHitbox.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var myhitedObject = other.GetComponent<IDestructible>();
        if (myhitedObject != null)
        {
            myhitedObject.destroyMe();
        }
    }
}
