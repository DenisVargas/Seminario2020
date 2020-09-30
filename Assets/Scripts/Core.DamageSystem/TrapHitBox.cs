using System;
using UnityEngine;
using Core.DamageSystem;

[Serializable, RequireComponent(typeof(Collider))]
public class TrapHitBox : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] bool _instaKill = true;
    [Space]
    [Header("Si instakill esta activado, no hace falta completar el resto :D")]
    [SerializeField] DamageType _damageType    = DamageType.blunt;
    [SerializeField] float _ammount            = 0f;
    [SerializeField] float _criticalMultiplier = 2f;

    Collider _col;

    private void Awake()
    {
        _col = GetComponent<Collider>();
    }

    public void SetTrapHitbox(bool instakill, DamageType type, float ammount, float criticalMultiplier)
    {
        _instaKill = instakill;
        _damageType = type;
        _ammount = ammount;
        _criticalMultiplier = criticalMultiplier;
    }

    public bool IsActive
    {
        get => _col.enabled;
        set => _col.enabled = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        var damageable = other.GetComponent<IDamageable<Damage, HitResult>>();
        if (damageable != null)
            damageable.GetHit(new Damage() { instaKill = _instaKill, Ammount = _ammount, criticalMultiplier = _criticalMultiplier, type = _damageType });
    }

    public void AE_TrapActivated()
    {
        _col.enabled = true;
    }
    public void AE_TrapDeactivated()
    {
        _col.enabled = false;
    }
}
