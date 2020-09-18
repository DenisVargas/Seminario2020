using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;

public class Jar : destroyable
{
    public Damage MyDamage;

    private void OnTriggerEnter(Collider collision)
    {
        var damagecomponent = collision.GetComponent<IDamageable<Damage, HitResult>>();
        if (damagecomponent != null)
        {
            GetHit(damagecomponent.GetDamageStats());
            damagecomponent.GetHit(GetDamageStats());
        }
    }
    public override Damage GetDamageStats()
    {
        return MyDamage;
    }
    public override HitResult GetHit(Damage damage)
    {

        if (damage.type == DamageType.hit || damage.type == DamageType.explotion || damage.type == DamageType.blunt)
        {
            destroyedObject.SetActive(true);
            notDestroyedObject.SetActive(false);
            Debug.Log("me rompi");

        }
        //if(damage.type == DamageType.e_fire)
        //{

        //    // aca pasa algo
        //}

        return new HitResult() { conected = true, fatalDamage = true };
    }
}
