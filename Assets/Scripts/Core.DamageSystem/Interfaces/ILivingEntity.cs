using System;
using UnityEngine;

public interface ILivingEntity
{
    GameObject gameObject { get; }

    //Permite subscibir una funcion que se llame cuando esta unidad muere.
    void SubscribeToLifeCicleDependency(Action<Collider> OnEntityDead);
    void UnsuscribeToLifeCicleDependency(Action<Collider> OnEntityDead);
}
