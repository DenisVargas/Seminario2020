using Core.DamageSystem;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Jail : MonoBehaviour
{
    [SerializeField] GameObject HitParticle;
    [SerializeField] Damage _toAplyDamage = new Damage();
    [SerializeField] Collider _hitbox = null;

    [SerializeField] float deactivateDelay = 1f;
    [SerializeField] bool autoDeactivate = false;
    Animator _anims;

#if UNITY_EDITOR

    [Header("DEBUG")]
    [SerializeField] bool debugThisJail = false;

#endif

    private void OnCollisionEnter(Collision collision)
    {
#if UNITY_EDITOR
        if (debugThisJail)
            print($"{gameObject.name} Entré en collisión we");
#endif
    }

    void Awake()
    {
        _anims = GetComponent<Animator>();
        if (_hitbox != null && !_hitbox.isTrigger)
            _hitbox.isTrigger = true;
    }

    public void Drop()
    {
        _anims.SetBool("Activated", true);
    }
    public void PullUp()
    {
        _anims.SetBool("Activated", false);
        if (_hitbox != null)
            _hitbox.enabled = false;
        HitParticle.SetActive(false);
    }

    public void AV_FallEnded()
    {
        HitParticle.SetActive(true);
        if (_hitbox != null)
            _hitbox.enabled = true;
        if (autoDeactivate)
            StartCoroutine(delayedDeactivate());
    }

    IEnumerator delayedDeactivate()
    {
        yield return new WaitForSeconds(deactivateDelay);
        PullUp();
    }
    private void OnTriggerEnter(Collider other)
    {
        var hurtBox = other.GetComponent<IDamageable<Damage,HitResult>>();
        if (hurtBox != null)
        {
            hurtBox.GetHit(_toAplyDamage);
        }
    }
}
