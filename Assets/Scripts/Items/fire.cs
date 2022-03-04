
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;

public class fire : MonoBehaviour
{
    public Damage MyDamage;
    // Start is called before the first frame update


    // Update is called once per frame

    //private void OnCollisionEnter(Collision collision)
    //{
    //    var damagecomponent = collision.gameObject.GetComponent<IDamageable<Damage, HitResult>>();
    //    if (damagecomponent != null)
    //    {
    //        Damage enemyCollisionDamage = new Damage() { type = DamageType.hit };
    //        damagecomponent.GetHit(MyDamage); //Causo daño por choque.
    //    }
    //}
    private void OnTriggerStay(Collider other)
    {
        var torch = other.GetComponent<Torch>();
        if (torch && torch.isEquiped) return;

        var damageable = other.GetComponent<IDamageable<Damage, HitResult>>();
        if (damageable != null)
            damageable.GetHit(MyDamage);
    }
}
