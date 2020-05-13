using UnityEngine;
using Core.DamageSystem;

[RequireComponent(typeof(Collider))]
public class TrapHitBox : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] bool _instaKill;
    [Space]
    [Header("Si instakill esta activado, no hace falta completar el resto :D")]
    [SerializeField] DamageType _damageType;
    [SerializeField] float _ammount;
    [SerializeField] float _criticalMultiplier;

    private void OnTriggerEnter(Collider other)
    {
        var hurtBox = other.GetComponent<HurtBox>();
        if (hurtBox != null)
        {
            hurtBox.TransferDamage(new Damage() { instaKill = _instaKill, Ammount = _ammount, criticalMultiplier = _criticalMultiplier, type = _damageType });
        }
    }
}
