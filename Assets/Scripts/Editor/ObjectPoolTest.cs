using NUnit.Framework;
using Utility.ObjectPools;
using System;


public class ObjectPoolTest
{
    public class instantiatonTest : IPoolObject
    {
        int value = 0;
        public bool enabled = false;
        public Pool<instantiatonTest> pool;

        public void setValue(int value)
        {
            this.value = value;
        }

        public void Disable()
        {
            enabled = false;
        }

        public void Enable()
        {
            enabled = true;
        }

        public void Dispose()
        {
            pool.ReturnToPool(this);
        }
    }

    [Test]
    [TestCase(3)]
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(0)]
    public void ObjectPool_CreatePool_PositiveParameter(int initialCount)
    {
        var Pool = new Pool<instantiatonTest>();
        Pool.Populate(initialCount, () => { return null; });

        Assert.That(Pool.Count == initialCount);
    }

    [Test]
    [TestCase(-3)]
    [TestCase(-2)]
    [TestCase(-1)]
    public void ObjectPool_CreatePool_NegativeParameter(int initialCount)
    {
        var Pool = new Pool<instantiatonTest>();
        Pool.Populate(initialCount, () => { return null; });

        Assert.That(Pool.Count == 0);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ObjectPool_SetAsDinamic(bool isdinamic)
    {
        var pool = new Pool<instantiatonTest>(isdinamic);
        pool.Populate(1, () => { return null; });

        Assert.That(pool.IsDinamic == isdinamic, "El pool no se ha marcado como corresponde");
    }

    [Test]
    public void ObjectPool_FactoryMethod_CheckInitialStock()
    {
        Func<instantiatonTest> factoryMethod = () =>
        {
            return new instantiatonTest();
        };
        var pool = new Pool<instantiatonTest>();
        pool.Populate(1, factoryMethod);

        //Execute
        var objectInitialized = pool.GetObject();
        bool isNotNull = objectInitialized != null;
        bool isEnabled = objectInitialized.enabled;
        bool countIsCorrect = pool.Count == 0;

        Assert.That(isNotNull && isEnabled && countIsCorrect, "El pool no genero el stock inicial.");
    }

    [Test]
    public void ObjectPool_ReturnPullObject()
    {
        var pool = new Pool<instantiatonTest>();
        Func<instantiatonTest> factory = () =>
        {
            var element = new instantiatonTest();
            element.pool = pool;
            return element;
        };
        pool.Populate(1, factory);

        instantiatonTest pooled = pool.GetObject();
        pooled.setValue(5);

        Assert.IsNotNull(pooled, "El pool retorno un objeto vacío.");
    }

    [Test]
    public void ObjectPool_CreatesUnderDemand()
    {
        var pool = new Pool<instantiatonTest>(true);
        Func<instantiatonTest> factory = () =>
        {
            var element = new instantiatonTest();
            element.pool = pool;
            return element;
        };
        pool.Populate(1, factory);

        pool.GetObject();
        pool.GetObject();
        var finalObject = pool.GetObject();

        Assert.IsNotNull(finalObject, "El pool retorno un objeto vacío.");
    }
}
