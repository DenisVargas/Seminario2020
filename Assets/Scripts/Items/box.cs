using UnityEngine;
using Core.DamageSystem;
using IA.PathFinding;

public class box :  destroyable
{
    [SerializeField] Damage MyDamage;
    [SerializeField] Node[] AffectedNodes = new Node[0];

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
    public override Damage GetDamageStats()
    {
        return MyDamage;
    }
    public override HitResult GetHit(Damage damage)
    {
        if (damage.type == DamageType.blunt)
        {
            destroyedObject.SetActive(true);
            notDestroyedObject.SetActive(false);
            foreach (var node in AffectedNodes)
                node.ChangeNodeState(NavigationArea.Navegable);
        }
        //if(damage.type == DamageType.e_fire)
        //{
        //    // aca pasa algo
        //}

        return new HitResult() { conected = true, fatalDamage = true };
    }
}

