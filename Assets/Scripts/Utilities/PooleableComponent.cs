using UnityEngine;
using Utility.ObjectPools;

public class PooleableComponent : MonoBehaviour, IPoolObject
{
    public Pool<IPoolObject> pool;

    /// <summary>
    /// LLama a esta función en vez de Destroy(GameObject);
    /// </summary>
    public void Dispose()
    {
        pool.ReturnToPool(this);
    }

    /// <summary>
    /// Callback que se llama cuando el item es "Sacado" del Pool.
    /// </summary>
    public virtual void Disable() { }
    /// <summary>
    /// Callback que se llama cuando el item es devuelto al Pool.
    /// </summary>
    public virtual void Enable() { }
}
