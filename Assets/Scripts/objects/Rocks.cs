using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;

public class Rocks : destroyable
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

        if (damage.type == DamageType.explotion)
        {
            destroyedObject.SetActive(true);
            notDestroyedObject.SetActive(false);
            

        }
        
        return new HitResult() { conected = true, fatalDamage = true };
    }
}
