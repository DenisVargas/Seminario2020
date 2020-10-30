using UnityEngine;
using Core.DamageSystem;

public class Jar : destroyable
{
    private void OnTriggerEnter(Collider collision)
    {
        var damagecomponent = collision.GetComponent<IDamageable<Damage, HitResult>>();
        if (damagecomponent != null)
        {
            GetHit(damagecomponent.GetDamageStats());
            damagecomponent.GetHit(GetDamageStats());
        }
    }

    public override HitResult GetHit(Damage damage)
    {
        var result = new HitResult(true);
        result.fatalDamage = true;

        if (damage.type == DamageType.hit || damage.type == DamageType.explotion || damage.type == DamageType.blunt)
        {
            //Debug.Log("me rompi");
            _destroyedObject.SetActive(true);
            _normalObject.SetActive(false);
        }

        return result;
    }
}
