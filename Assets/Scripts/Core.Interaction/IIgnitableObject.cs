using UnityEngine;

public interface IIgnitableObject
{
    bool lockInteraction { get; }
    bool isFreezed { get; set; }
    bool Burning { get; }
    bool IsActive { get; }
    GameObject gameObject { get; }

    void Ignite(float delayTime);
    void OnInteractionEvent();
}