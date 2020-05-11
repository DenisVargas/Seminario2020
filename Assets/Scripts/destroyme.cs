using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyme : MonoBehaviour, IDestructible
{
    public void destroyMe()
    {
        Debug.Log("entre");
        Destroy(gameObject);
    }
}
