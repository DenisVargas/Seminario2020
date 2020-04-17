using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    e_fire,
    cutting,
    blunt,
    piercing
}

public struct Damage
{
    public DamageType type;
    public float Ammount;
    public float criticalMultiplier;
    public bool instaKill;
}

public interface IDamageable
{
    float health { get; set; }
}

[System.Serializable]
public struct DamageModifier
{
    public DamageType type;
    public float percentual;
}

//Calculo de daño y manejo del Collider.
//Aqui podremos setear debilidades.

[RequireComponent(typeof(Collider))]
public class DamageDealer : MonoBehaviour
{
    [SerializeField] IDamageable _body;

    [SerializeField] DamageModifier[] weaknesses;  //Aumentan el daño multiplicandolo x un porcentaje.
    [SerializeField] DamageModifier[] resistances; //reducen el daño x el un porcentaje.

    Collider    _col;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _body = GetComponentInParent<IDamageable>();
    }

    public void GetDamage(Damage damage)
    {
        if (damage.instaKill)
        {
            _body.health = 0;
            return;
        }

        _body.health -= damage.Ammount;
    }
}
