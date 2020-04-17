using UnityEngine;

public interface IIgnitableObject
{
    bool IsIgniteable { get; }
    GameObject gameObject { get; }

    void Ignite(float delayTime);
}