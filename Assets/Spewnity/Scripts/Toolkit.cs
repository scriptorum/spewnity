using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// TODO Add StartCoroutine versions of the Lerps? I'll have to extend them off MonoBehaviour then.
// TODO Should tween functions should check if object is null and end coroutine if the case? Does that lead to bug masking?
// TODO LerpColor should support null endColor if AnimationCurve is specified
// TODO Rewrite Join using LINQ, dammit
// TODO Tests for GetParent/GetObject
namespace Spewnity
{
    public static class Toolkit
    {
        /// <summary>
        /// Tweens the transform from its current position to endPos in world space.
        /// </summary>
        /// <param name="tform">The transform to tween</param>
        /// <param name="endPos">If endPos is null, curve is required and treated as a current position multipler. Z is not affected. Otherwise tweens between current and endPos, optionally using 0-1 AnimationCurve to adjust easing.</param>
        /// <param name="duration">Duration of lerp in seconds</param>
        /// <param name="curve">Optional easing curve</param>
        /// <param name="onComplete">Action triggered at end of tween. Example Action: (t) => Debug.Log("Transform complete!")</param>
        /// <returns>Coroutine suitable for passing to StartCoroutine()</returns>
        public static IEnumerator LerpPosition(this Transform tform, Vector3? endPos, float duration,
            AnimationCurve curve = null, System.Action<Transform> onComplete = null)
        {
            Vector3 startPos = tform.position;
            if (endPos == null && curve == null) throw new UnityException("endPos or curve can be null, but not both");

            float t = 0;
            while (t < 1)
            {
                yield return null;
                t += Time.deltaTime / duration;
                if (endPos == null)
                {
                    float et = curve.Evaluate(t);
                    tform.position = new Vector3(startPos.x * et, startPos.y * et, startPos.z);
                }
                else tform.position = Vector3.Lerp(startPos, (Vector3) endPos, (curve == null ? t : curve.Evaluate(t)));
            }

            if (onComplete != null) onComplete.Invoke(tform);
        }

        /// <summary>
        /// Tweens the transform from its current scale to endScale in local space.
        /// </summary>
        /// <param name="tform">The transform to tween</param>
        /// <param name="endScale">If endScale is null, curve is required and treated as a current scale multipler. Z is not affected. Otherwise tweens between current and endScale, optionally using 0-1 AnimationCurve to adjust easing.</param>
        /// <param name="duration">Duration of lerp in seconds</param>
        /// <param name="curve">Optional easing curve</param>
        /// <param name="onComplete">Action triggered at end of tween. Example Action: (t) => Debug.Log("Transform complete!")</param>
        /// <returns>Coroutine suitable for passing to StartCoroutine()</returns>
        public static IEnumerator LerpScale(this Transform tform, Vector3? endScale, float duration,
            AnimationCurve curve = null, System.Action<Transform> onComplete = null)
        {
            Vector3 startScale = tform.localScale;
            if (endScale == null && curve == null) throw new UnityException("endScale or curve can be null, but not both");

            float t = 0;
            while (t < 1)
            {
                yield return null;
                t += Time.deltaTime / duration;
                if (endScale == null)
                {
                    float et = curve.Evaluate(t);
                    tform.localScale = new Vector3(startScale.x * et, startScale.y * et, startScale.z);
                }
                else tform.localScale = Vector3.Lerp(startScale, (Vector3) endScale, (curve == null ? t : curve.Evaluate(t)));
            }

            if (onComplete != null) onComplete.Invoke(tform);
        }

        /// <summary>
        /// Tweens the between two colors. Sends the interpolated color to onUpdate().
        /// </summary>
        /// <param name="startColor">The initial color to tween from</param>
        /// <param name="endColor">The final color to tween to</param>
        /// <param name="duration">Duration of lerp in seconds</param>
        /// <param name="onUpdate">Callback that receives the tweened color. LerpColor cannot tween-in-place, so use this function to assign the color as needed.</param>
        /// <param name="curve">Optional easing curve</param>
        /// <param name="onComplete">Action triggered at end of tween. Example Action: (t) => Debug.Log("Transform complete!")</param>
        /// <returns>Coroutine suitable for passing to StartCoroutine()</returns>
        public static IEnumerator LerpColor(this Color startColor, Color endColor, float duration, System.Action<Color> onUpdate,
            AnimationCurve curve = null, System.Action onComplete = null)
        {
            float t = 0;
            while (t < 1)
            {
                yield return null;
                t += Time.deltaTime / duration;
                onUpdate(Color.Lerp((Color) startColor, endColor, (curve == null ? t : curve.Evaluate(t))));
            }

            if (onComplete != null) onComplete.Invoke();
        }

        /// <summary>
        /// Tweens the between two float values. Sends the interpolated float to onUpdate().        
        /// </summary>
        /// <param name="startValue">The initial value to tween from</param>
        /// <param name="endValue">The final value to float to</param>
        /// <param name="duration">Duration of lerp in seconds</param>
        /// <param name="onUpdate">Callback that receives the tweened float. LerpFloat cannot tween-in-place, so use this function to assign the float as needed.</param>
        /// <param name="curve">Optional easing curve</param>
        /// <param name="onComplete">Action triggered at end of tween. Example Action: (t) => Debug.Log("Transform complete!")</param>
        /// <returns>Coroutine suitable for passing to StartCoroutine()</returns>
        public static IEnumerator LerpFloat(this float startValue, float endValue, float duration, System.Action<float> onUpdate,
            AnimationCurve curve = null, System.Action onComplete = null)
        {
            float t = 0;
            while (t < 1)
            {
                yield return null;
                t += Time.deltaTime / duration;
                onUpdate(Mathf.Lerp(startValue, endValue, (curve == null ? t : curve.Evaluate(t))));
            }

            if (onComplete != null) onComplete.Invoke();
        }

        /// <summary>
        /// Snaps an Vector3 to a specified angle
        /// </summary>
        /// <param name="vec">Vector3</param>
        /// <param name="snapAngle">The angle to snap, in degrees</param>
        /// <returns>A Vector3, rounded to the nearest multiple of the snap angle, in degrees</returns>
        public static Vector3 SnapTo(this Vector3 vec, float snapAngle)
        {
            float x = Mathf.Round(vec.x / snapAngle) * snapAngle;
            float y = Mathf.Round(vec.y / snapAngle) * snapAngle;
            float z = Mathf.Round(vec.z / snapAngle) * snapAngle;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Shuffles an array or list in place. Also returns the input.
        /// </summary>
        public static IList<T> Shuffle<T>(this IList<T> ilist)
        {
            for (int i = ilist.Count - 1; i > 0; i--)
            {
                int r = Random.Range(0, i + 1);
                if (r != i)
                {
                    T tmp = ilist[i];
                    ilist[i] = ilist[r];
                    ilist[r] = tmp;
                }
            }
            return ilist;
        }

        /// <summary>
        /// Integer-based absolute value
        /// </summary>
        public static int Abs(this int val)
        {
            return val < 0 ? -val : val;
        }

        /// <summary>
        /// Integer based sign
        /// </summary>
        public static int Sign(this int val)
        {
            return val < 0 ? -1 : (val == 0 ? 0 : 1);
        }

        /// <summary>
        /// Integer based Max
        /// </summary>
        public static int Max(this int val1, int val2)
        {
            return val1 > val2 ? val1 : val2;
        }

        /// <summary>
        /// Integer based Min
        /// </summary>
        public static int Min(this int val1, int val2)
        {
            return val1 < val2 ? val1 : val2;
        }

        /// <summary>
        /// Returns the absolute path to the Transform
        /// </summary>
        public static string GetFullPath(this Transform o)
        {
            if (o.parent == null) return "/" + o.name;
            return o.parent.GetFullPath() + "/" + o.name;
        }

        /// <summary>
        /// Returns the absolute path to the GameObject
        /// </summary>
        public static string GetFullPath(this GameObject o)
        {
            return GetFullPath(o.transform);
        }

        public static T Shift<T>(this IList<T> collection)
        {
            T val = collection[0];
            collection.RemoveAt(0);
            return val;
        }
        
        public static IList<T> Unshift<T>(this IList<T> collection, T val)
        {
            collection.Insert(0, val);
            return collection;
        }

        public static IList<T> Pop<T>(this IList<T> collection, T val)
        {
            collection.Add(val);
            return collection;
        }

        public static T Unpop<T>(this IList<T> collection)
        {
            int idx = collection.Count - 1;
            T val = collection[idx];
            collection.RemoveAt(idx);
            return val;
        }

        // 
        /// <summary>
        /// Returns one random object from the list or array 
        /// </summary>
        /// <para>
        /// Throws an assert if the collection is empty
        /// </para>
        public static T Rnd<T>(this IList<T> collection)
        {
            Debug.Assert(collection.Count > 0);
            return collection.ElementAt(Random.Range(0, collection.Count));
        }

        /// <summary>
        /// Returns true or false randomly 
        /// </summary>
        public static bool CoinFlip()
        {
            return (Random.value >= 0.5f);
        }

        /// <summary>
        /// Returns a random color
        /// </summary>
        /// <param name="includeAlpha">If true, alpha is also randomized; otherwise alpha is 1.0 (opaque)</param>
        /// <returns>the color</returns>
        public static Color RandomColor(bool includeAlpha = false)
        {
            return new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                includeAlpha ? Random.Range(0f, 1f) : 1
            );
        }

        /// <summary>
        /// Swaps two values, just awful, I hate myself for writing it 
        /// </summary>
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        /// <summary>
        /// Joins an array or list of some type into a comma separated string
        /// </summary>
        public static string Join<T>(this IList<T> iList, string delim = ",")
        {
            if (iList == null || delim == null)
                throw new System.ArgumentException();

            string ret = "";
            foreach (T t in iList)
            {
                if (ret != "")
                    ret += delim;
                ret += t.ToString();
            }
            return ret;
        }

        /// <summary>
        /// Capitalizes the first character of the string.
        /// </summary>
        /// <param name="str">The string to capitalize</param>
        /// <returns>The changed string</returns>
        public static string ToInitCase(this string str)
        {
            if (str.IsEmpty())
                return str;
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }

        /// <summary>
        /// Capitalizes each word of the string.
        /// </summary>
        /// <param name="str">The string to title case</param>
        /// <returns>The changed string</returns>
        public static string ToTitleCase(this string str)
        {
            if (str.IsEmpty())
                return str;
            string result = "";
            foreach (string part in Regex.Split(str, @"([A-Za-z0-9]+)"))
                result += part.ToInitCase();
            return result;
        }

        /// <summary>
        /// True if the string is null or has no content
        /// </summary>
        public static bool IsEmpty(this string str)
        {
            return (str == null || str == "");
        }

        /// <summary>
        /// Returns the named child object of the Transform.
        /// <para>Throws an ArgumentException if not found</para>
        /// </summary>
        public static Transform GetChild(this Transform tform, string name)
        {
            Transform child = tform.Find(name);
            if (name == null || child == null)
                throw new System.ArgumentException("Could not find child " + name + " under " + tform.name);
            return child;
        }

        /// <summary>
        /// Returns the named child object of the GameObject.
        /// <para>Throws an ArgumentException if not found</para>
        /// </summary>
        public static GameObject GetChild(this GameObject go, string name)
        {
            return go.transform.GetChild(name).gameObject;
        }

        /// <summary>
        /// Returns all game objects in the scene 
        /// </summary>
        public static List<GameObject> GetAllObjects()
        {
            List<GameObject> list = new List<GameObject>();

            GameObject[] allGO = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (GameObject go in allGO)
            {
                if (go.activeInHierarchy)
                    list.Add(go);
            }

            return list;
        }

        /// <summary>
        /// Returns the typed component of the object pointed to by the path. 
        /// <para>Throws an error if the object or component cannot be found.</para>
        /// </summary>
        /// <example>E.g., Camera cam = Toolkit.GetComponentOf&lt;Camera&gt;("/Camera");</example>
        public static T GetComponentOf<T>(string path)
        {
            GameObject go = GameObject.Find(path);
            if (go == null)
                throw new UnityException("Cannot find GameObject at path " + path);
            T component = go.GetComponent<T>();
            if (component == null)
                throw new UnityException("Cannot find component " + typeof(T).ToString() + " in GameObject at path " + path);
            return component;
        }

        /// <summary>
        /// Destroys all children underneath this Transform, but does
        /// not destroy the Transform's GameObject itself.
        /// </summary>
        public static void DestroyChildren(this Transform tform)
        {
            foreach (Transform child in tform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Destroys all children underneath this GameObject, but does
        /// not destroy the Transform's GameObject itself.
        /// </summary>
        public static void DestroyChildren(this GameObject go)
        {
            go.transform.DestroyChildren();
        }

        // 
        /// <summary>
        /// Same as DestroyChildren, but does so immediately 
        /// <see cref="DestroyChildren"/>
        /// </summary>
        public static void DestroyChildrenImmediately(this Transform tform)
        {
            for (int i = tform.childCount - 1; i >= 0; i--)
                GameObject.DestroyImmediate(tform.GetChild(i).gameObject);
        }

        /// <summary>
        /// Throws a NullReferenceException if this object is null, with an optional message 
        /// <example>Camera c = GetComponent&lt;Camera&gt;(); c.ThrowIfNull();</example>
        /// </summary>
        public static void ThrowIfNull(this object o, string msg = "ArgumentNullException")
        {
            if (o == null || o.Equals(null))
                throw new UnityException(msg);
        }

        /// <summary>
        /// Instantiates a GameObject from a prefab, assigns its parent, and optionally
        /// sets its position relative to its parent.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate</param>
        /// <param name="parent">The parent to assign</param>
        /// <param name="position">Optionally, the position to assign, relative to parent; if not supplied, position will be zero</param>
        /// <returns>The instantiated object</returns>
        public static GameObject CreateChild(this GameObject prefab, Transform parent, Vector3? position = null)
        {
            GameObject go = GameObject.Instantiate(prefab, parent);
            go.transform.localPosition = (position == null ? Vector3.zero : (Vector3) position);
            return go;
        }

        /// <summary>
        /// Instatiates a GameObject from a prefab, sets its position in world space, 
        /// and optionally assigns its parent.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate</param>
        /// <param name="position">The position to assign, in worldspace</param>
        /// <param name="parent">Optionally, the parent to assign</param>
        /// <returns></returns>
        public static GameObject Create(this GameObject prefab, Vector3 position, Transform parent = null)
        {
            GameObject go = GameObject.Instantiate(prefab, position, Quaternion.identity);
            go.transform.parent = parent;
            return go;
        }

        /// <summary>
        /// Returns the total visual bounds of a GameObject, including all its children
        /// </summary>
        /// <param name="go">The game object</param>
        /// <param name="includeInactive">Should inactive children be included in the bounds?</param>
        /// <returns>The full bounds</returns>
        public static Bounds GetBounds(this GameObject go, bool includeInactive = false)
        {
            Bounds bounds = new Bounds();
            bool boundsInitialized = false;

            foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>(includeInactive))
            {
                if (!boundsInitialized)
                {
                    bounds = renderer.bounds;
                    boundsInitialized = true;
                }
                else bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

#if UNITY_EDITOR        
        /// <summary>
        /// Returns the object associated with the SerializedProperty.
        /// </summary>
        /// <param name="prop">The SerializedProperty</param>
        /// <returns>An object, ready to cast, e.g.: prop.GetObject() as MyPropObject</returns>
        public static object GetObject(this SerializedProperty prop)
        {
            return GetObjectField(GetParent(prop), prop.name);
        }

        /// <summary>
        /// Returns the object associated with the parent of the SerializedProperty.
        /// <para>Thanks @whydoidoit: http://answers.unity3d.com/questions/425012/get-the-instance-the-serializedproperty-belongs-to.html</para>
        /// </summary>
        /// <param name="prop">The SerializedProperty</param>
        /// <returns>An object, ready to cast, e.g.: prop.GetObject() as MyPropObject</returns>
        public static object GetParent(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = Toolkit.GetArrayValue(obj, elementName, index);
                }
                else obj = Toolkit.GetObjectField(obj, element);
            }
            return obj;
        }

        /// <summary>
        /// Uses reflection to return the object's field member.
        /// </summary>
        /// <param name="source">A class object</param>
        /// <param name="name">The name of a member field</param>
        /// <returns>The associated object</returns>
        public static object GetObjectField(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null)
            {
                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null)
                    return null;
                return p.GetValue(source, null);
            }
            return f.GetValue(source);
        }

        /// <summary>
        /// Returns the value of the a specific index of an untyped enumerable object.
        /// </summary>
        /// <param name="source">A class object</param>
        /// <param name="name">The name of an enumerable member field (array, etc)</param>
        /// <param name="index">The value at the index of the enumerable member</param>
        /// <returns>The associated value</returns>
        public static object GetArrayValue(object source, string name, int index)
        {
            var enumerable = Toolkit.GetObjectField(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
                enm.MoveNext();
            return enm.Current;
        }
#endif
    }
}