using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Jail : MonoBehaviour
{
    [SerializeField] Collider PhysicalCollider = null;
    Rigidbody _rb;

    public bool Growndchecked = false;

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
        if(collision.collider.gameObject.layer == 0)
        {
            Growndchecked = true;
        }

        var myhitedObject = collision.collider.GetComponent<IDestructible>();
        if (myhitedObject != null)
        {
            myhitedObject.destroyMe();
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
