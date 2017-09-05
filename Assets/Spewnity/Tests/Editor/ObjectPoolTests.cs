using System.Collections;
using NUnit.Framework;
using Spewnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

// TODO GameObjectPool tests
// TODO PoolManager tests
// TODO ComponentPool tests
public class ObjectPoolTests
{
    [Test]
    public void TestDefaultMinSize()
    {
        ObjectPool<Foo> pool = new ObjectPool<Foo>();
        pool.Populate();
        Assert.AreEqual(pool.available.Count, pool.minSize);
    }

    [Test]
    public void TestMinSize()
    {
        ObjectPool<Foo> pool = new ObjectPool<Foo>(10, 20, 1);
        pool.Populate();
        Assert.AreEqual(pool.minSize, 10);
        Assert.AreEqual(pool.available.Count, 10);
    }

    [Test]
    public void TestMaxSize()
    {
        ObjectPool<Foo> pool = new ObjectPool<Foo>(0, 4, 1);
        Assert.AreNotEqual(pool.TryGet(), null);
        Assert.AreNotEqual(pool.TryGet(), null);
        Assert.AreNotEqual(pool.TryGet(), null);
        Assert.AreNotEqual(pool.TryGet(), null);
        Assert.AreEqual(pool.TryGet(), null);
        Assert.AreEqual(pool.maxSize, 4);
        Assert.AreEqual(pool.available.Count, 0);
        Assert.AreEqual(pool.busy.Count, 4);
    }

    [Test]
    public void TestGrowRate()
    {
        ObjectPool<Foo> pool = new ObjectPool<Foo>(0, 30, 3);
        Assert.That(pool.available.Count == 0);
        Assert.That(pool.TryGet() != null);
        Assert.AreEqual(pool.available.Count, 2);
        Assert.AreEqual(pool.busy.Count, 1);
    }

    [Test]
    public void TestGet()
    {
        ObjectPool<Foo> pool = new ObjectPool<Foo>(1, 1, 1);
        Assert.AreEqual(pool.available.Count, 0);
        pool.Populate();
        Assert.AreEqual(pool.available.Count, 1);
        Assert.AreEqual(pool.busy.Count, 0);
        Foo foo = pool.Get();
        Assert.AreEqual(pool.available.Count, 0);
        Assert.AreEqual(pool.busy.Count, 1);
        Assert.NotNull(foo);
        Assert.AreEqual(pool.available.IndexOf(foo), -1);
        Assert.AreNotEqual(pool.busy.IndexOf(foo), -1);
    }

    [Test]
    public void TestRelease()
    {
        ObjectPool<Foo> pool = new ObjectPool<Foo>(5, 5, 1, true);
        Foo foo = pool.Get();
        pool.Release(foo);
        Assert.AreEqual(pool.available.Count, 5);
        Assert.AreEqual(pool.busy.Count, 0);
        Assert.AreNotEqual(pool.available.IndexOf(foo), -1);
        Assert.AreEqual(pool.busy.IndexOf(foo), -1);
    }
}

internal class Foo
{
    static int counter = 0;
    public int i;
    public Foo()
    {
        i = Foo.counter++;
    }
}