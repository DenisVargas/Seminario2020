using Core.DamageSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyable : MonoBehaviour, IDamageable<Damage, HitResult>
{
    public GameObject destroyedObject;
    public GameObject notDestroyedObject;
    protected virtual void Awake()
    {
        destroyedObject.SetActive(false);
        notDestroyedObject.SetActive(true);
    }

    public bool IsAlive => throw new System.NotImplementedException();

    public void FeedDamageResult(HitResult result)
    {
        throw new System.NotImplementedException();
    }

    public virtual Damage GetDamageStats()
    {
        return new Damage();
    }

    public virtual HitResult GetHit(Damage damage)
    {
        destroyedObject.SetActive(true);
        notDestroyedObject.SetActive(false);
        Debug.Log("me rompi");
        //if(damage.type == DamageType.blunt)
        //{
        //    destroyedObject.SetActive(true);
        //    notDestroyedObject.SetActive(false);
        //    Debug.Log("me rompi");
        //}
        //if(damage.type == DamageType.e_fire)
        //{
        //    // aca pasa algo
        //}

        return new HitResult() { conected = true, fatalDamage = true };
    }
    public void GetStun(Vector3 AgressorPosition, int PosibleKillingMethod)
    {
        throw new System.NotImplementedException();
    }
}
