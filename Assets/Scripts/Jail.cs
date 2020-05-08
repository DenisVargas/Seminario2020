using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;

[RequireComponent(typeof(Collider))]
public class Jail : MonoBehaviour
{
    [SerializeField] Collider PhysicalCollider;
    [SerializeField] Collider _damageDealer;
    Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
    }

    public void Drop()
    {
        _rb.isKinematic = false;
        StartCoroutine(Deactivate());
    }

    private void OnCollisionEnter(Collision collision)
    {
        var myhitedObject = collision.collider.GetComponent<IDestructible>();

        if (myhitedObject != null)
        {
            myhitedObject.destroyMe();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var killable = other.GetComponentInParent<IDamageable<Damage>>();
        if (killable != null)
        {
            killable.Hit(new Damage() { instaKill = true });
        }
    }

    IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(10f);
        PhysicalCollider.enabled = false;
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
