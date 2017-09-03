using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Spewnity;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Spewnity
{
    public class ObjectPooler : MonoBehaviour
    {
        /// <summary>
        /// Returns the sole or primary instance of ObjectPooler. See primaryInstance.
        /// </summary>
        public static ObjectPooler instance;

        [Reposition("name", "prefab")]
        [Tooltip("All of the GameObjectPools managed by the ObjectPooler")]
        public List<GameObjectPool> pools;
        
        [Tooltip("One ObjectPooler is assigned to ObjectPooler.instance; if true (and this is the only ObjectPooler so marked) this will be the instance")]
        public bool primaryInstance;

        /// <summary>
        /// General awakey stuff. Maintains the static instance, populates the pools with their minimums.
        /// </summary>
        void Awake()
        {
            if (primaryInstance)
            {
                if (instance != null)
                    Debug.LogWarning("Multiple primary instances exist in the scene, only one can be assigned to ObjectPooler.instance");
                instance = this;
            }
            else if (instance == null)
                instance = this;

            // Spawn the minimum number of objects specified
            foreach (GameObjectPool p in pools)
                p.Populate();
        }

        /// <summary>
        /// Returns the GameObjectPool associated with the specified name
        /// </summary>
        /// <param name="poolName">The pool you want</param>
        /// <returns>A GameObjectPool, which you can manipulate directly</returns>
        public GameObjectPool GetPool(string poolName)
        {
            GameObjectPool pool = pools.Find(t => t.name == poolName);
            if (pool == null)
                throw new UnityException("Cannot find pool '" + poolName + "'");
            return pool;
        }

        /// <summary>
        /// Retrieves a GameObject from the specified pool.
        /// </summary>
        /// <param name="poolName">The pool that has the GameObjects you want</param>
        /// <returns>The GameObject requested</returns>
        public GameObject Get(string poolName)
        {
            return GetPool(poolName).Get();
        }

        /// <summary>
        /// Retrieves a GameObject from the specified pool.
        /// </summary>
        /// <param name="poolName">The pool that has the GameObjects you want</param>
        /// <returns>The GameObject requested, or null if none are available</returns>
        public GameObject TryGet(string poolName)
        {
            return GetPool(poolName).TryGet();
        }

        /// <summary>
        /// Releases the hold on a GameObject, and recycles it back into the specified pool.
        /// </summary>
        /// <param name="poolName">The pool from where you got the GameObject</param>
        /// <param name="go">The GameObject to release</param>
        public void Release(string poolName, GameObject go)
        {
            GetPool(poolName).Release(go);
        }

        /// <summary>
        /// Releases all in-use GameObjects from ALL pools.
        /// </summary>
        public void ReleaseAll()
        {
            foreach (GameObjectPool p in pools)
                p.ReleaseAll();
        }

        /// <summary>
        /// Removes ALL GameObjects from ALL pools, whether they are in-use or otherwise available.
        /// </summary>
        public void Clear()
        {
            foreach (GameObjectPool p in pools)
                p.Clear();
        }

        /// <summary>
        /// Provides an uninitialized GameObject with default settings.
        /// </summary>
        void OnValidate()
        {
            foreach (GameObjectPool p in pools)
            {
                if (!p.initialized)
                {
                    p.minSize = 0;
                    p.maxSize = -1;
                    p.growRate = 1;
                    p.initialized = true;
                    p.parent = this.transform;
                }
            }
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A GameObjectPool can be used directly by your class, or through an ObjectPooler.
    /// <para>See also ObjectPool<T> for general information on pool mechanics.</para>
    /// </summary>
    [System.Serializable]
    public class GameObjectPool : ObjectPool<GameObject>
    {
        [Tooltip("The name of the pool; used to name newly created GameObjects; also used by Object Pooler to reference this pool")]
        public string name;

        [Tooltip("The prefab GameObject that all newly created instances will be instantiated from")]
        public GameObject prefab;

        [Tooltip("The parent of returned GameObjects; if null, GameObjects are parented by root")]
        public Transform parent;

        [Tooltip("Displays the status of the pool objects in the inspector")]
        public PoolStatus poolStatus;

        [Tooltip("Events! We all love events!")]
        public PoolEvents events;
        
        [HideInInspector]
        public bool initialized; // Used by ObjectPooler

        /// <summary>
        /// Instantiates a new GameObject for the pool
        /// </summary>
        protected override GameObject Create()
        {
            if (prefab == null)
                throw new UnityException("Missing prefab for pool '" + name + "'");
            GameObject go = GameObject.Instantiate(prefab);
            go.name = name + "#" + (Size + 1);
            go.SetActive(false);
            go.transform.SetParent(parent, false);
            events.Create.Invoke(go);
            return go;
        }

        /// <summary>
        /// GameObject-specific preparations before returning a usable GameObject.
        /// </summary>
        /// <param name="go">The GameObject being returned</param>
        protected override void Prepare(GameObject go)
        {
            go.SetActive(true);
            events.Prepare.Invoke(go);
        }

        /// <summary>
        /// GameObject-speficic object recycling, when a GameObject is released from use.
        /// </summary>
        /// <param name="go">The GameObject being recycled</param>
        protected override void Recycle(GameObject go)
        {
            go.transform.SetParent(parent, false);
            go.SetActive(false);
            events.Recycle.Invoke(go);
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    [System.Serializable]
    public struct PoolEvents
    {
        [Tooltip("Called when the pool grows, and creates a new object for it; this could also be at the start")]
        public PoolEvent Create;

        [Tooltip("Called when the user requests an object using Get, and it is being prepared for use")]
        public PoolEvent Prepare;

        [Tooltip("Called when the user has returned an object using Release, and the object is being recycled")]
        public PoolEvent Recycle;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Just a dummy object for displaying the pool's available/used status in the inspector
    /// </summary>
    [System.Serializable]
    public struct PoolStatus
    {
        public int dummy;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    [System.Serializable]
    public class PoolEvent : UnityEvent<GameObject> { }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A generic object pool for any kind of object.
    /// <para>Use TryGet to retrieve an item from the pool, and Release to return it. TryGet returns null if no object is available.</para>
    /// <para>Use Get to ensure you retrieve an item from the pool; if there are none free, the oldest object will be repurposed.</para>
    /// </summary>
    [System.Serializable]
    public class ObjectPool<T> where T : class
    {
        [Tooltip("The number of objects to create when the pool is created")]
        public int minSize = 5;

        [Tooltip("The maximum number of objects in the pool; -1 means unlimited, 0 caps any further growth")]
        public int maxSize = UNLIMITED;

        [Tooltip("The number of objects to create when any objects are created")]
        public int growRate = 5;

        [HideInInspector]
        [Tooltip("The objects in the pool ready to be used")]
        public List<T> available = new List<T>();

        [HideInInspector]
        [Tooltip("The objects from the pool that are busy and waiting to be released")]
        public List<T> busy = new List<T>();

        [Tooltip("If the maxSize is unlimited, there is no limit to the total number that can be created")]
        public const int UNLIMITED = -1;

        [Tooltip("If the maxSize is capped, no more objects will be created")]
        public const int CAPPED = 0;

        /// <returns>The total number of created objects, whether in-use or not</returns>
        public int Size { get { return available.Count + busy.Count; } }

        public ObjectPool() { }

        /// <summary>
        /// Constructor with parameters.
        /// </summary>
        /// <param name="min">The minimum size of the pool</param>
        /// <param name="max">The maximum size of the pool; a size of -1 means unlimited, a size of 0 disables all pool growth</param>
        /// <param name="grow">The rate to grow the pool when additional objects are needed</param>
        /// <param name="populateNow">If true, grows the pool to its minimum size; otherwise call Populate() to do this</param>
        public ObjectPool(int min, int max, int grow, bool populateNow = false)
        {
            this.minSize = min;
            this.maxSize = max;
            this.growRate = grow;

            if (populateNow)
                Populate();
        }

        /// <summary>
        /// Populates the pool with the minimum size.
        /// </summary>
        public void Populate()
        {
            Debug.Assert(growRate > 0);
            Debug.Assert(minSize >= 0);
            Debug.Assert(maxSize >= -1);
            if (maxSize > 0)
                Debug.Assert(minSize <= maxSize);

            if (Size < minSize)
                Grow(minSize - Size);
        }

        /// <summary>
        /// Obtains an object from the pool. Releases the oldest busy object if none are available.
        /// </summary>
        /// <returns>The object</returns>
        public T Get()
        {
            T obj = TryGet();
            if (obj == null && busy.Count > 0)
            {
                Release(busy[0]);
                obj = TryGet();
            }

            return obj;
        }

        /// <summary>
        /// Obtains an object from the pool
        /// </summary>
        /// <returns>The object, or null if no objects are available</returns>
        public T TryGet()
        {
            if (available.Count == 0 && Grow(growRate) <= 0)
                return null;

            T obj = available[0];
            available.RemoveAt(0);
            busy.Add(obj);
            Prepare(obj);
            return obj;
        }

        /// <summary>
        /// Returns the object to the pool (see Get/TryGet)
        /// </summary>
        /// <param name="obj">The object to release</param>
        public void Release(T obj)
        {
            busy.Remove(obj);
            Recycle(obj);
            available.Add(obj);
        }

        /// <summary>
        /// Returns all in-use objects back to the pool
        /// </summary>
        public void ReleaseAll()
        {
            while (busy.Count > 0)
                Release(busy[0]);
        }

        /// <summary>
        /// Removes all objects from the pool, whether utilized or not.
        /// <para>Does NOT call Release on used objects! So they will remain in your scene!</para>
        /// </summary>
        public void Clear()
        {
            available.Clear();
            busy.Clear();
        }

        /// <summary>
        /// Grows the pool by a certain amount, but not beyond the maximum size.
        /// </summary>
        /// <param name="count">The number of objects to create in the pool</param>
        /// <returns>The amount grown; could be zero!</returns>
        private int Grow(int count)
        {
            int grown = 0;
            while (count-- > 0)
            {
                if (Size >= maxSize && maxSize > UNLIMITED)
                    break;

                available.Add(Create());
                grown++;
            }
            return grown;
        }

        /// <summary>
        /// Creates a new object for the pool.
        /// <para>Override this to provide your own constructed object and set its initial parameters</para>
        /// </summary>
        protected virtual T Create()
        {
            return (T) System.Activator.CreateInstance(typeof (T));
        }

        /// <summary>
        /// Prepares the requested object for return to the user
        /// <para>Called by Get() and TryGet()</para>
        /// </summary>
        /// <param name="obj">The object to prepare</param>
        protected virtual void Prepare(T obj) { }

        /// <summary>
        /// Prepares the recently-released object for return to the pool.
        /// <para>Called by Release()</para>
        /// </summary>
        /// <param name="obj">The object to recycle</param>
        protected virtual void Recycle(T obj) { }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof (PoolStatus))]
    public class PoolStatusPD : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if(prop == null)
                return;
            
            string parentPath = prop.propertyPath.Substring(0, prop.propertyPath.LastIndexOf("."));
            SerializedProperty gopProp = prop.serializedObject.FindProperty(parentPath);            

            float x = EditorGUIUtility.labelWidth + pos.x;
            float h = EditorGUIUtility.singleLineHeight;
            float y = pos.yMax - h;
            EditorGUI.LabelField(new Rect(pos.x, y, pos.width, h), "Pool Status");

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            int maxSize = gopProp.FindPropertyRelative("maxSize").intValue;
            int availableSize = gopProp.FindPropertyRelative("available").arraySize;
            int busySize = gopProp.FindPropertyRelative("busy").arraySize;
            string str = availableSize + " ready, " + busySize + " busy, " + (maxSize <= GameObjectPool.UNLIMITED ? 
                "unlimited" : (maxSize - (availableSize + busySize)).ToString() + " potential");
            GUIStyle style = new GUIStyle(GUI.skin.label);
            GUIContent gc = new GUIContent(str);
            Vector2 labelBounds = style.CalcSize(gc);
            EditorGUI.DrawRect(new Rect(x, y, labelBounds.x, labelBounds.y), new Color(.7f, .7f, .8f));
            EditorGUI.LabelField(new Rect(x, y, labelBounds.x, labelBounds.y), gc, style);
            EditorGUI.indentLevel = indent;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
#endif
}