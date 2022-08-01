using UnityEngine;
using IA.PathFinding;
using System.Collections;
using System.Collections.Generic;

public class DestroyableBox :  Destroyable
{
    //private void OnTriggerEnter(Collider collision)
    //{
    //    print("Box recieved Damage");
    //    var damagecomponent = collision.GetComponent<IDamageable<Damage, HitResult>>();
    //    if(damagecomponent != null)
    //    {
    //       GetHit(damagecomponent.GetDamageStats());
    //        damagecomponent.GetHit(GetDamageStats());
    //    }
    //}
    [Header("Visual Elements")]
    public List<GameObject> OnTopVisualElements = new List<GameObject>();

    public override HitResult GetHit(Damage damage)
    {
        HitResult result = new HitResult(true);

        if (damage.type == DamageType.blunt)
        {
            result.fatalDamage = true;
            getSmashed();
        }

        if (damage.type == DamageType.Fire)
        {
            result.ignited = true;
            Burn();
        }

        if (damage.type == DamageType.explotion)
        {
            result.exploded = true;
            Explode(damage.explotionOrigin, damage.explotionForce);
        }

        return result;
    }

    void getSmashed()
    {
        //print($"{gameObject.name} ha sido aplastado.");

        ReplaceToDestroyedMesh();
        foreach (var node in AffectedNodes)
            node.ChangeNodeState(NavigationArea.Navegable);
    }

    void Explode(Vector3 explotionOrigin, float explotionForce)
    {
        //print($"{gameObject.name} ha sido destruido por una explosión.");
        ReplaceToDestroyedMesh();
        StartCoroutine(delayedDestroy(_timeToDestroy));
    }

    void Burn()
    {
        ReplaceToDestroyedMesh();
        if(OnTopVisualElements.Count > 0)
        {
            foreach (var item in OnTopVisualElements)
                Destroy(item);
        }
        StartCoroutine(delayedDestroy(_timeToDestroy));
    }
}

