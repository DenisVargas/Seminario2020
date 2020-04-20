using System;
using System.Collections.Generic;

namespace Utility.ObjectPools
{
    public interface IPoolObject
    {
        /// <summary>
        /// Usamos esta función para devolver nuestro objeto a su correspondiente Pool.
        /// Idealmente tendremos una referencia al pool, añadida durante la creación.
        /// </summary>
        void Dispose();
        /// <summary>
        /// Callback que se llama cuando el item es "Sacado" del Pool.
        /// </summary>
        void Enable();
        /// <summary>
        /// Callback que se llama cuando el item es devuelto al Pool.
        /// </summary>
        void Disable();
    }

    public class Pool<T> where T : IPoolObject
    {
        Queue<IPoolObject> pool = new Queue<IPoolObject>();
        public Func<IPoolObject> FactoryMethod = delegate { return default; };

        public int Count { get => pool.Count; }
        public bool IsDinamic { get; set; } = false;

        public Pool( bool IsDinamic = false)
        {
            this.IsDinamic = IsDinamic;
        }

        public void Populate(int initialCount, Func<IPoolObject> FactoryMethod)
        {
            this.FactoryMethod = FactoryMethod;

            for (int i = 0; i < initialCount; i++)
            {
                var pOb = FactoryMethod();
                pOb.Disable();
                pool.Enqueue(pOb);
            }
        }

        public T GetObject()
        {
            if (pool.Count == 0)
            {
                if (IsDinamic)
                {
                    var onDemandPO = FactoryMethod();
                    onDemandPO.Enable();
                    return (T)onDemandPO;
                }

                return default;
            }

            var pOb = pool.Dequeue();
            pOb.Enable();
            return (T)pOb;
        }

        public void ReturnToPool(T pOb)
        {
            pOb.Disable();
            pool.Enqueue(pOb);
        }
    }
}

