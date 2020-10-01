using UnityEngine;
using Core.DamageSystem;

public class Spikes : MonoBehaviour
{
    [SerializeField] Damage toApplyDamage = new Damage();
    [SerializeField] Collider _hitbox = null;

#if UNITY_EDITOR
    [SerializeField] bool debugThisSpike = false;
#endif

    private void OnTriggerStay(Collider other)
    {

#if UNITY_EDITOR
        if (debugThisSpike)
        {
            Debug.Log($"{gameObject.name} está en colisión con {other.gameObject.name}");
        } 
#endif

        var damageable = other.GetComponent<IDamageable<Damage, HitResult>>();
        if (damageable != null)
            damageable.GetHit(toApplyDamage);
    }

    void AE_TrapActivated()
    {
        if (_hitbox != null)
            _hitbox.enabled = true;
    }
    void AE_TrapDeactivated()
    {
        if (_hitbox != null)
            _hitbox.enabled = false;
    }
}
