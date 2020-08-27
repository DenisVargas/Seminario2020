using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDestructible 
{
    Vector3 position { get; }
    Transform transform { get; }
    void destroyMe();
}
