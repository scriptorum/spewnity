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
        public static ObjectPooler instance;

        public bool primaryInstance;

        [Reposition("name", "prefab")]
        public List<GameObjectPool> pools;

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

            foreach(GameObjectPool p in pools) p.Init(this.transform);
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
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    [System.Serializable]
    public class GameObjectPool : ObjectPool<GameObject>
    {
        public string name;
        public GameObject prefab;
        public PoolStatus poolStatus;
        public PoolEvents events;
        private Transform autoParent;

        public void Init(Transform autoParent)
        {
            this.autoParent = autoParent;
            base.Init();
        }

        /// <summary>
        /// Instantiates a new GameObject for the pool
        /// </summary>
        protected override GameObject Create()
        {
            if (prefab == null)
                throw new UnityException("Missing prefab for pool '" + name + "'");
            GameObject go = GameObject.Instantiate(prefab);
            go.name = name + "#" + (Size + 1);
            go.transform.parent = autoParent;
            go.SetActive(false);
            events.Create.Invoke(go);
            return go;
        }

        protected override void Prepare(GameObject go)
        {
            go.SetActive(true);
            events.Prepare.Invoke(go);
        }

        protected override void Recycle(GameObject go)
        {
            go.transform.parent = autoParent;
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
    public class PoolStatus
    {
        public string who_you_calling_a_dummy;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    [System.Serializable]
    public class PoolEvent : UnityEvent<GameObject> { }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    [System.Serializable]
    public class ObjectPool<T> where T : class
    {
        [Tooltip("The number of objects to create when the pool is created")]
        public int minSize = 5;

        [Tooltip("The maximum number of objects in the pool; 0 means unlimited")]
        public int maxSize = 32;

        [Tooltip("The number of objects to create when any objects are created")]
        public int growRate = 5;

        [HideInInspector]
        [Tooltip("The objects in the pool ready to be used")]
        public List<T> available = new List<T>();

        [HideInInspector]
        [Tooltip("The objects from the pool that are busy and waiting to be released")]
        public List<T> busy = new List<T>();

        public int Size { get { return available.Count + busy.Count; } }

        /// <summary>
        /// Constructor without parameters. Creates a new empty pool.
        /// <para>When using this constructor, you must call Init() to initialize the pool</para>
        /// </summary>
        public ObjectPool() { }

        /// <summary>
        /// Constructor with parameters. Creates a new pool at the minSize specified.
        /// </summary>
        /// <param name="min">The minimum size of the pool</param>
        /// <param name="max">The maximum size of the pool; a size of 0 means unlimited</param>
        /// <param name="grow">The rate to grow the pool when additional objects are needed</param>
        public ObjectPool(int min, int max, int grow)
        {
            this.minSize = min;
            this.maxSize = max;
            this.growRate = grow;
            Init();
        }

        /// <summary>
        /// Initializes the pool to minimum size; this must be called when using ObjectPool with the parameterless constructor
        /// </summary>
        public void Init()
        {
            Debug.Assert(growRate > 0);
            Debug.Assert(minSize >= 0);
            Debug.Assert(maxSize == 0 || minSize <= maxSize);

            Grow(minSize);
        }

        /// <summary>
        /// Obtains an object from the pool. Releases the oldest busy object if none are available.
        /// </summary>
        /// <returns>The object</returns>
        public T Get()
        {
            T obj = TryGet();
            if (obj == null)
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
            if (available.Count == 0)
            {
                if (maxSize < 0 || busy.Count < maxSize)
                    Grow(growRate);
                else return null;
            }
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
        /// Grows the pool by a certain amount, but not beyond the maximum size.
        /// </summary>
        /// <param name="count">The number of objects to create in the pool</param>
        private void Grow(int count)
        {
            while (count-- > 0)
            {
                if (Size >= maxSize)
                    break;

                available.Add(Create());
            }
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
            float x = EditorGUIUtility.labelWidth + pos.x;
            float h = EditorGUIUtility.singleLineHeight;
            float y = pos.yMax - h;
            EditorGUI.LabelField(new Rect(pos.x, y, pos.width, h), "Pool Status");

            ObjectPooler pooler = (ObjectPooler) prop.serializedObject.targetObject;
            Match match = Regex.Match(prop.propertyPath, @"\[(\d+)\]\.poolStatus$");
            GameObjectPool pool = pooler.pools[int.Parse(match.Groups[1].Value)];

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            string str = pool.available.Count + " ready, " +
                pool.busy.Count + " busy, " +
                (pool.maxSize <= 0 ? "Inf" : (pool.maxSize - pool.Size).ToString()) + " potential";
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