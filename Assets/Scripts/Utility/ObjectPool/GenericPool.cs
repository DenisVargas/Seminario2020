using System;
using System.Collections.Generic;

namespace Utility.ObjectPools.Generic
{
    public class PoolObject<T>
    {
        public T GetObject { get; }

        public PoolObject(T _object)
        {
            GetObject = _object;
        }
    }

    public class GenericPool<T>
    {
        Queue<PoolObject<T>> _inactiveObjets;
        Dictionary<T, PoolObject<T>> _activeObjects;

        Action<T> _init       = delegate { };
        Action<T> _finit      = delegate { };
        Func<T> _factoyMethod = delegate { return default(T); };
        bool _isDinamic        = false;

        public GenericPool(bool isDinamic = false)
        {
            _inactiveObjets = new Queue<PoolObject<T>>();
            _activeObjects = new Dictionary<T, PoolObject<T>>();

            _init = delegate { };
            _finit = delegate { };
            _factoyMethod = delegate { return default(T); };
            _isDinamic = isDinamic;
        }

        public GenericPool(int stockInicial, Func<T> FactoryMethod, Action<T> Initialize, Action<T> Finalize, bool isDinamic = false)
        {
            _activeObjects = new Dictionary<T, PoolObject<T>>();
            _inactiveObjets = new Queue<PoolObject<T>>();

            _factoyMethod = FactoryMethod;
            _init = Initialize;
            _finit = Finalize;
            _isDinamic = isDinamic;

            for (int i = 0; i < stockInicial; i++)
            {
                _inactiveObjets.Enqueue(new PoolObject<T>(_factoyMethod()));
            }
        }

        public GenericPool<T> SetFactoryMethod(Func<T> factoryMethod)
        {
            _factoyMethod = factoryMethod;
            return this;
        }
        public GenericPool<T> SetInitMethod(Action<T> initialize)
        {
            _init = initialize;
            return this;
        }
        public GenericPool<T> SetFinitMethod(Action<T> finit)
        {
            _finit = finit;
            return this;
        }
        public void AddStock(int stock, bool isDinamic = false)
        {
            _isDinamic = isDinamic;

            for (int i = 0; i < stock; i++)
            {
                _inactiveObjets.Enqueue(new PoolObject<T>(_factoyMethod()));
            }
        }

        public T GetObjectFromPool()
        {
            if (_inactiveObjets.Count > 0)
            {
                var poolObject = _inactiveObjets.Dequeue();
                _init(poolObject.GetObject);
                _activeObjects.Add(poolObject.GetObject, poolObject);
                return poolObject.GetObject;
            }

            if (_isDinamic)
            {
                var poolObject = new PoolObject<T>(_factoyMethod());
                _init(poolObject.GetObject);
                _activeObjects.Add(poolObject.GetObject, poolObject);
                return poolObject.GetObject;
            }

            return default(T);
        }
        public void DisablePoolObject(T obj)
        {
            if (_activeObjects.ContainsKey(obj))
            {
                _finit(obj);
                _inactiveObjets.Enqueue(_activeObjects[obj]);
                _activeObjects.Remove(obj);
            }
        }
    }
}
