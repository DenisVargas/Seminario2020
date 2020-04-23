using UnityEngine;

public interface IIgnitableObject
{
    bool Burning { get; }
    bool IsActive { get; }
    GameObject gameObject { get; }

    void Ignite(float delayTime);
}