using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyme : MonoBehaviour, IDestructible
{
    public Vector3 position => transform.position;

    public void destroyMe()
    {
        Debug.Log("entre");
        Destroy(gameObject);
    }
}
