using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Jail : MonoBehaviour
{
    Animator _anims;
    TrapHitBox _hitbox = null;
    Collider _destructibleHitbox = null;
    public GameObject Smash;

    [SerializeField] float deactivateDelay = 1f;
    [SerializeField] bool autoDeactivate = false;

    [Header("Hitbox Settings")]
    [SerializeField] bool _instaKill = true;
    [SerializeField] DamageType _damageType = DamageType.blunt;
    [SerializeField] float _ammount = 0f;
    [SerializeField] float _criticalMultiplier = 2f;

    void Awake()
    {
        _anims = GetComponent<Animator>();
        _destructibleHitbox = GetComponent<Collider>();
        _destructibleHitbox.isTrigger = true;
        _hitbox = GetComponentInChildren<TrapHitBox>();
        if (_hitbox != null)
        {
            _hitbox.SetTrapHitbox(_instaKill, _damageType, _ammount, _criticalMultiplier);
        }
    }

    public void Drop()
    {
        _anims.SetBool("Activated", true);
    }
    public void PullUp()
    {
        _anims.SetBool("Activated", false);
        if (_hitbox != null)
        {
            _hitbox.IsActive = false;
        }
        if (_destructibleHitbox != null)
        {
            _destructibleHitbox.enabled = false;
        }
        Smash.SetActive(false);
    }

    public void AV_FallEnded()
    {
        Smash.SetActive(true);
        if (_hitbox != null)
        {
            _hitbox.IsActive = true;
        }
        if (_destructibleHitbox != null)
        {
            _destructibleHitbox.enabled = true;
        }
        if (autoDeactivate)
        {
            StartCoroutine(delayedDeactivate());
        }
        
    }

    IEnumerator delayedDeactivate()
    {
        yield return new WaitForSeconds(deactivateDelay);
        PullUp();
    }

   
}
