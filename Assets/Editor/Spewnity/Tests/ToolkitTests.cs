﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Spewnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

// TODO The four Lerp coroutines, DestroyChildren
public class ToolkitTests
{
    [UnityTest]
    public IEnumerator TestDestroyChildrenImmediately()
    {
        GameObject a = new GameObject("a");
        GameObject b = new GameObject("b");
        GameObject c = new GameObject("c");
        b.transform.parent = a.transform;
        c.transform.parent = a.transform;

        yield return null;
        Assert.AreEqual(2, a.transform.childCount);

        a.transform.DestroyChildrenImmediately();
        Assert.AreEqual(0, a.transform.childCount);
    }

    [Test]
    public void TestGetComponentOf()
    {
        GameObject a = new GameObject("a");
        GameObject b = new GameObject("b");
        b.transform.parent = a.transform;
        b.AddComponent<Foo>();
        Assert.IsNotNull(Toolkit.GetComponentOf<Foo>("/a/b"));
        Assert.AreEqual(25, (Toolkit.GetComponentOf<Foo>("/a/b")).value);
    }

    [Test]
    public void TestGetAllObjects()
    {
        List<GameObject> list = Toolkit.GetAllObjects();
        int initialCount = list.Count;

        GameObject a = new GameObject("a");
        GameObject b = new GameObject("b");
        b.transform.parent = a.transform;
        GameObject c = new GameObject("c");

        list = Toolkit.GetAllObjects();
        Assert.AreEqual(3 + initialCount, list.Count);
        Assert.Contains(a, list);
        Assert.Contains(b, list);
        Assert.Contains(c, list);
    }

    [Test]
    public void TestGetChild()
    {
        GameObject a = new GameObject("a");
        GameObject b = new GameObject("b");
        b.transform.parent = a.transform;
        Assert.AreEqual(b, a.GetChild("b"));
        Assert.AreEqual(b.transform, a.transform.GetChild("b"));
        Assert.Throws(typeof (System.ArgumentException), () => a.transform.GetChild("c"));
        Assert.Throws(typeof (System.ArgumentException), () => a.transform.GetChild(null));
    }

    [Test]
    public void TestCreateChild()
    {
        GameObject root = new GameObject("root");
        root.transform.position = new Vector3(10, 5, 1);
        GameObject prefab = new GameObject("prefab");
        GameObject o1 = prefab.CreateChild(root.transform);
        Assert.AreEqual(root.transform, o1.transform.parent);
        Assert.AreEqual(Vector3.zero, o1.transform.localPosition);
        Assert.AreEqual(10, o1.transform.position.x);
        Assert.AreEqual(5, o1.transform.position.y);
        Assert.AreEqual(1, o1.transform.position.z);
        GameObject o2 = prefab.CreateChild(root.transform, new Vector3(1, 1, 1));
        Assert.AreEqual(11, o2.transform.position.x);
        Assert.AreEqual(6, o2.transform.position.y);
        Assert.AreEqual(2, o2.transform.position.z);
    }

    [Test]
    public void TestCreate()
    {
        GameObject root = new GameObject("root");
        GameObject prefab = new GameObject("prefab");
        GameObject o1 = prefab.Create(new Vector3(5, 10, 0));
        Assert.AreEqual(5, o1.transform.position.x);
        Assert.AreEqual(10, o1.transform.position.y);
        Assert.AreEqual(0, o1.transform.position.z);
        Assert.IsNull(o1.transform.parent);
        GameObject o2 = prefab.Create(new Vector3(8, 12, 1), root.transform);
        Assert.AreEqual(8, o2.transform.position.x);
        Assert.AreEqual(12, o2.transform.position.y);
        Assert.AreEqual(1, o2.transform.position.z);
        Assert.AreEqual(root.transform, o2.transform.parent);
    }

    [Test]
    public void TestGetFullPath()
    {
        GameObject a = new GameObject("a");
        GameObject b = new GameObject("b");
        b.transform.parent = a.transform;
        GameObject c = new GameObject("c");
        c.transform.parent = b.transform;
        Assert.AreEqual("/a", a.GetFullPath());
        Assert.AreEqual("/a/b", b.GetFullPath());
        Assert.AreEqual("/a/b/c", c.GetFullPath());
        Assert.AreEqual("/a", a.transform.GetFullPath());
        Assert.AreEqual("/a/b", b.transform.GetFullPath());
        Assert.AreEqual("/a/b/c", c.transform.GetFullPath());
    }

    [Test]
    public void TestToolkitAbs()
    {
        int i = 10;
        int ni = -10;
        Assert.AreEqual(10, i.Abs());
        Assert.AreEqual(10, ni.Abs());
        Assert.AreEqual(5, (-5).Abs());
        Assert.AreEqual(5, 5. Abs());
        Assert.AreEqual(0, 0. Abs());
    }

    [Test]
    public void TestToolkitSign()
    {
        int i = 10;
        int ni = -10;
        Assert.AreEqual(1, i.Sign());
        Assert.AreEqual(-1, ni.Sign());
        Assert.AreEqual(-1, (-5).Sign());
        Assert.AreEqual(1, 5. Sign());
        Assert.AreEqual(0, 0. Sign());
    }

    [Test]
    public void TestToolkitMax()
    {
        Assert.AreEqual(5, 5. Max(3));
        Assert.AreEqual(8, 5. Max(8));
        Assert.AreEqual(5, 5. Max(5));
        Assert.AreEqual(-3, (-5).Max(-3));
        Assert.AreEqual(-5, (-5).Max(-8));
        Assert.AreEqual(-5, (-5).Max(-5));
        Assert.AreEqual(15, Toolkit.Max(10, 15));
    }

    [Test]
    public void TestToolkitMin()
    {
        Assert.AreEqual(3, 5. Min(3));
        Assert.AreEqual(5, 5. Min(8));
        Assert.AreEqual(5, 5. Min(5));
        Assert.AreEqual(-5, (-5).Min(-3));
        Assert.AreEqual(-8, (-5).Min(-8));
        Assert.AreEqual(-5, (-5).Min(-5));
        Assert.AreEqual(10, Toolkit.Min(10, 15));
    }

    [Test]
    public void TestToInitCase()
    {
        Assert.AreEqual("Big jim", "big jim".ToInitCase());
        Assert.AreEqual("Big-jim IN your FACE", "big-jim IN your FACE".ToInitCase());
        Assert.AreEqual("AAA", "AAA".ToInitCase());
        Assert.AreEqual("", "".ToInitCase());
        string n = null;
        Assert.AreEqual(null, n.ToInitCase());
    }

    [Test]
    public void TestToTitleCase()
    {
        Assert.AreEqual("Big Jim", "big jim".ToTitleCase());
        Assert.AreEqual("Big-Jim IN Your FACE", "big-jim IN your FACE".ToTitleCase());
        Assert.AreEqual("AAA", "AAA".ToTitleCase());
        Assert.AreEqual("A_Variable_With_Cheese", "a_variable_with_cheese".ToTitleCase());
        Assert.AreEqual("Turk182was A Bad!Movie", "turk182was a bad!movie".ToTitleCase());
        Assert.AreEqual("", "".ToTitleCase());
        string n = null;
        Assert.AreEqual(null, n.ToTitleCase());
    }

    [Test]
    public void TestIsEmpty()
    {
        Assert.AreEqual(false, "hi".IsEmpty());
        Assert.AreEqual(true, "".IsEmpty());
        Assert.AreEqual(false, " ".IsEmpty());
        string n = null;
        Assert.AreEqual(true, n.IsEmpty());
    }

    [Test]
    public void TestSwap()
    {
        int a = 5;
        int b = 10;
        Assert.AreEqual(5, a);
        Assert.AreEqual(10, b);
        Toolkit.Swap(ref a, ref b);
        Assert.AreEqual(5, b);
        Assert.AreEqual(10, a);

        string c = "c";
        string d = null;
        Assert.AreEqual("c", c);
        Assert.AreEqual(null, d);
        Toolkit.Swap(ref c, ref d);
        Assert.AreEqual("c", d);
        Assert.AreEqual(null, c);
    }

    [Test]
    public void TestJoin()
    {
        System.Collections.Generic.List<int> intList = new System.Collections.Generic.List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Assert.AreEqual("1,2,3,4,5,6,7,8,9", intList.Join());
        Assert.AreEqual("123456789", intList.Join(""));
        Assert.AreEqual("1__2__3__4__5__6__7__8__9", intList.Join("__"));

        string[] strArray = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        Assert.AreEqual("1,2,3,4,5,6,7,8,9", strArray.Join());
        Assert.AreEqual("123456789", strArray.Join(""));
        Assert.AreEqual("1__2__3__4__5__6__7__8__9", strArray.Join("__"));

        bool[] nullArray = null;
        Assert.Throws(typeof (System.ArgumentException), () => strArray.Join(null));
        Assert.Throws(typeof (System.ArgumentException), () => nullArray.Join());
    }

    [Test]
    public void TestThrowIfNull()
    {
        string zeroStr = "";
        Assert.DoesNotThrow(() => zeroStr.ThrowIfNull());
        string nullStr = null;
        Assert.Throws(typeof (UnityException), () => nullStr.ThrowIfNull());
        GameObject go = null;
        Assert.Throws(typeof (UnityException), () => go.ThrowIfNull());
        go = new GameObject();
        Assert.DoesNotThrow(() => go.ThrowIfNull());
    }

    [Test]
    public void TestShuffle()
    {
        System.Collections.Generic.List<int> intList = new System.Collections.Generic.List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        System.Collections.Generic.List<int> intListOrig = new System.Collections.Generic.List<int>(intList);
        intList.Shuffle();
        Assert.AreNotEqual(intListOrig, intList); // this can (very rarely) return a false negative
        Assert.AreEqual(intListOrig.Count, intList.Count);
        int sum = 0;
        intList.ForEach((t) => sum += t);
        Assert.AreEqual(45, sum);

        string[] strArray = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        Assert.AreEqual("123456789", strArray.Join(""));
        strArray.Shuffle();
        Assert.AreNotEqual("123456789", strArray.Join("")); // this can (very rarely) return a false negative, enjoy
        Assert.AreEqual(9, strArray.Length);
    }

    [Test]
    public void TestRnd()
    {
        System.Collections.Generic.List<int> intList = new System.Collections.Generic.List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        int[] intArray = new int[9];
        int loops = 100;
        for (int i = 0; i < loops; i++)
        {
            int val = intList.Rnd();
            intArray[val - 1]++;
            Assert.AreEqual(true, intList.Contains(val));
        }

        // Weak ass std deviation test
        float sum1 = 0;
        foreach(int i in intArray) sum1 += i;
        float mean = sum1 / intArray.Length;
        Assert.AreEqual(((float) loops) / intList.Count, mean);
        float sum2 = 0;
        foreach(int i in intArray) sum2 += Mathf.Pow(i - mean, 2);
        float stdev = Mathf.Sqrt(sum2 / intList.Count);
        Assert.Less(stdev, mean);
        Assert.Greater(stdev, 0);
    }

    [Test]
    public void TestCoinFlip()
    {
        int result = 0;
        for (int i = 0; i < 100; i++)
            result += Toolkit.CoinFlip() ? 1 : 0;
        Debug.Log(result);

        // May occasionally get a false positive, but more interations = zzzzzzz
        Assert.Greater(result, 10);
        Assert.Less(result, 90);
    }

    [Test]
    public void TestRandomColor([NUnit.Framework.Range(0, 20)] int xx)
    {
        Color c = Toolkit.RandomColor(false);
        Assert.AreEqual(1.0f, c.a);
        Assert.GreaterOrEqual(c.r, 0);
        Assert.GreaterOrEqual(c.g, 0);
        Assert.GreaterOrEqual(c.b, 0);
        Assert.LessOrEqual(c.r, 1);
        Assert.LessOrEqual(c.g, 1);
        Assert.LessOrEqual(c.b, 1);
        c = Toolkit.RandomColor(true);
        Assert.GreaterOrEqual(c.a, 0);
        Assert.LessOrEqual(c.a, 1);
    }

    [Test]
    public void TestSnap()
    {
        Vector3 original = new Vector3(-48, 197, 82);
        Vector3 snap45 = original.SnapTo(45);
        Assert.AreEqual(-45, snap45.x);
        Assert.AreEqual(180, snap45.y);
        Assert.AreEqual(90, snap45.z);
        Vector3 snap30 = original.SnapTo(30);
        Assert.AreEqual(-60, snap30.x);
        Assert.AreEqual(210, snap30.y);
        Assert.AreEqual(90, snap30.z);
    }

    internal class Foo : MonoBehaviour { public int value = 25; }
}