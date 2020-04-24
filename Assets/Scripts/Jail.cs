using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jail : MonoBehaviour
{
    Rigidbody _rb;
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
    }

    public void Drop()
    {
        _rb.isKinematic = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var myhitedObject = collision as IDestructible;

        if (myhitedObject != null)
        {
            myhitedObject.destroyMe();
        }
    }
}
