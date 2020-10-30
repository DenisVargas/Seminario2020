using UnityEngine;
using IA.PathFinding;

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
}

