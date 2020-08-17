using UnityEngine;
using Utility.ObjectPools;

public class PooleableComponent : MonoBehaviour, IPoolObject<PooleableComponent>
{
    public Pool<PooleableComponent> pool;

    /// <summary>
    /// LLama a esta función en vez de Destroy(GameObject);
    /// </summary>
    public void Dispose()
    {
        if (pool != null)
            pool.ReturnToPool(this);
        else
            Debug.LogError("Pool no seteado");
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
