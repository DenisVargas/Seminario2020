using UnityEngine;
using Core.DamageSystem;

public class Jar : Destroyable
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
            ReplaceToDestroyedMesh();
            StartCoroutine(delayedDestroy(_timeToDestroy));
        }

        return result;
    }
}
