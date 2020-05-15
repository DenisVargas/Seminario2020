using UnityEngine;
using Core.DamageSystem;

[RequireComponent(typeof(Collider))]
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

    public bool IsActive
    {
        get => _col.enabled;
        set => _col.enabled = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        var hurtBox = other.GetComponent<HurtBox>();
        if (hurtBox != null)
        {
            hurtBox.TransferDamage(new Damage() { instaKill = _instaKill, Ammount = _ammount, criticalMultiplier = _criticalMultiplier, type = _damageType });
        }
    }
}
