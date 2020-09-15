using UnityEngine;
using Core.DamageSystem;
public class box :  destroyable
{
    
    public Damage MyDamage;

   
    private void OnTriggerEnter(Collider collision)
    {
        var damagecomponent = collision.GetComponent<IDamageable<Damage, HitResult>>();
        if(damagecomponent != null)
        {
           GetHit(damagecomponent.GetDamageStats());
            damagecomponent.GetHit(GetDamageStats());
        }
    }
    public override Damage GetDamageStats()
    {
        return MyDamage;
    }
}

