using UnityEngine;
using IA.PathFinding;
using System.Collections;

public class box :  destroyable
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

        return result;
    }

    void getSmashed()
    {
        print($"{gameObject.name} ha sido aplastado.");

        _destroyedObject.SetActive(true);
        _normalObject.SetActive(false);
        foreach (var node in AffectedNodes)
            node.ChangeNodeState(NavigationArea.Navegable);
    }

    void Explode(Vector3 explotionOrigin, float explotionForce)
    {
        print($"{gameObject.name} ha sido destruido por una explosión.");
    }

    void Burn()
    {
        _destroyedObject.SetActive(true);
        _normalObject.SetActive(false);
        onDestroy(gameObject);
        onDestroy = delegate { };
        StartCoroutine(delayedDestroy());
    }

    IEnumerator delayedDestroy()
    {
        yield return new WaitForSeconds(4f);
        Destroy(gameObject);
    }
}

