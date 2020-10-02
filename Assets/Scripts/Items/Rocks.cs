using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;
using Core.InventorySystem;

public class Rocks : destroyable
{
    public LayerMask Hiteables = ~0;
    public Damage MyDamage;
    public bool isFlying = false;

    [SerializeField] Collider toIgnore = null;
    [SerializeField] Collider _hitbox = null;
    [SerializeField] Item itemComp = null;

#if UNITY_EDITOR
    [SerializeField] bool debugThisRock = false;
#endif

    protected override void Awake()
    {
        base.Awake();
        if (itemComp)
        {
            //El owner de un item es ignorado cuando ocurre una colisión!
            itemComp.OnSetOwner += (owner) => { toIgnore = owner; };
            //Al ejecutarse Throw en un item, el estado pasa a flying.
            itemComp.OnThrowItem += () => { isFlying = true; };
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

#if UNITY_EDITOR
        if (debugThisRock)
            print($"CondComp::Colisioné con algo wey: {collision.gameObject.name}");
#endif

        if (isFlying)
        {
            if (collision.collider == toIgnore) return;

            var damagecomponent = collision.gameObject.GetComponent<IDamageable<Damage, HitResult>>();
            if (damagecomponent != null)
            {
                Damage enemyCollisionDamage = new Damage() { type = DamageType.hit };
                GetHit(enemyCollisionDamage); //Recibo daño por choque.
                damagecomponent.GetHit(MyDamage); //Causo daño por choque.
            } 
        }
    }
    public override HitResult GetHit(Damage damage)
    {
        if (damage.type == DamageType.explotion || damage.type == DamageType.hit)
        {
            destroyedObject.SetActive(true);
            destroyedObject.transform.SetParent(null);
            notDestroyedObject.SetActive(false);
            StartCoroutine(DelayedDestroy(2f));
            _hitbox.enabled = false;
        }

        return new HitResult() { conected = true, fatalDamage = true };
    }
    IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
