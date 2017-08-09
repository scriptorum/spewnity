﻿//TODO Add StartCoroutine versions of the Lerps? I'll have to extend them off MonoBehaviour then.
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spewnity
{
    public static class Toolkit
    {
        /**
         * Tweens the transform from its current position to endPos in world space.
         * Remember to wrap in a StarCoroutine().
         * Action triggered at end of tween. Example Action: (t) => Debug.Log("Transform complete!")
         * If endPos is null, curve is required and treated as a current position multipler. Z is not affected.
         * Otherwise tweens between current and endPos, optionally using 0-1 AnimationCurve to adjust easing.
         * TODO Should tween functions should check if object is null and end coroutine if the case? Does that lead to bug masking?
         */
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

        /**
         * Tweens the transform from its current scale to endScale in local space.
         * Remember to wrap in a StarCoroutine().
         * Action triggered at end of tween. Example Action: (t) => Debug.Log("Transform complete!")
         * If endScale is null, curve is required and treated as a current scale multipler. Z is not affected.
         * Otherwise tweens between current and endScale, optionally using 0-1 AnimationCurve to adjust easing.
         * TODO Should tween functions should check if object is null and end coroutine if the case? Does that lead to bug masking?
         **/
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

        // Tweens the between two colors. Sends the interpolated color to onUpdate().
        // Remember to wrap in a StarCoroutine().
        // TODO Support null endColor if AnimationCurve is specified
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

        // Tweens the between two float values. Sends the interpolated float to onUpdate().
        // Remember to wrap in a StarCoroutine().
        // TODO Support null endColor if AnimationCurve is specified
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

        // Snaps the XY component of a Vector3 to a 45 degree angle
        public static Vector3 Snap45(this Vector3 v3, float snapAngle)
        {
            float angle = Vector3.Angle(v3, Vector3.up);
            if (angle < snapAngle / 2.0f) // Cannot do cross product 
                return Vector3.up * v3.magnitude; //   with angles 0 & 180
            if (angle > 180.0f - snapAngle / 2.0f) return Vector3.down * v3.magnitude;

            float t = Mathf.Round(angle / snapAngle);
            float deltaAngle = (t * snapAngle) - angle;

            Vector3 axis = Vector3.Cross(Vector3.up, v3);
            Quaternion q = Quaternion.AngleAxis(deltaAngle, axis);
            return q * v3;
        }

        // Shuffles an array or list in place. Also returns the input.
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

        // Integer-based absolute value
        public static int Abs(this int val)
        {
            return val < 0 ? -val : val;
        }

        // Integer based sign
        public static int Sign(this int val)
        {
            return val < 0 ? -1 : (val == 0 ? 0 : 1);
        }

        // Returns the absolute path to the Transform
        public static string GetFullPath(this Transform o)
        {
            if (o.parent == null) return "/" + o.name;
            return o.parent.GetFullPath() + "/" + o.name;
        }

        // Returns the absolute path to the GameObject
        public static string GetFullPath(this GameObject o)
        {
            return GetFullPath(o.transform);
        }

        // Returns one random object from the list or array
        // Throws an assert if the collection is empty
        public static T Rnd<T>(this IList<T> collection)
        {
            Debug.Assert(collection.Count > 0);
            return collection.ElementAt(Random.Range(0, collection.Count));
        }

        // Returns true or false randomly
        public static bool CoinFlip()
        {
            return (Random.value >= 0.5f);
        }

        // Swaps two values, just awful, I hate myself for writing it
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        // Joins an array or list of some type into a comma separated string
        // TODO Learn some damn LINQ will you
        public static string Join<T>(this IList<T> iList, string delim = ",")
        {
            string ret = "";
            foreach(T t in iList)
            {
                if (ret != "")
                    ret += ",";
                ret += t.ToString();
            }
            return ret;
        }

        // True if the string is null or has no content
        public static bool IsEmpty(this string str)
        {
            return (str == null || str == "");
        }

        // Returns the named child object of the transform, throws an assert if not found
        public static Transform GetChild(this Transform tform, string name)
        {
            Transform child = tform.Find(name);
            Debug.Assert(child != null, "Could not find child " + name + " under " + tform.name);
            return child;
        }

        // Returns the named child object of the gameobject, throws an assert if not found
        public static GameObject GetChild(this GameObject go, string name)
        {
            return go.transform.GetChild(name).gameObject;
        }

        // Returns all game objects in the scene
        public static List<GameObject> GetAllObjects()
        {
            List<GameObject> list = new List<GameObject>();

            GameObject[] allGO = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach(GameObject go in allGO)
            {
                if (go.activeInHierarchy)
                    list.Add(go);
            }

            return list;
        }

        // Returns the typed component of the object pointed to by the path.
        // Throws an assert if the object or component cannot be found.
        // E.g., Camera cam = Toolkit.GetComponentOf<Camera>("/Camera");
        public static T GetComponentOf<T>(string path)
        {
            GameObject go = GameObject.Find(path);
            Debug.Assert(go != null);
            T component = go.GetComponent<T>();
            Debug.Assert(component != null);
            return component;
        }

        // Destroys all children underneath this transform, but does
		// not destroy the transform's GameObject itself.
        public static void DestroyChildren(this Transform tform)
        {
            foreach(Transform child in tform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        // Destroys all children underneath this GameObject, but does
		// not destroy this GameObject itself.
        public static void DestroyChildren(this GameObject go)
		{
			go.transform.DestroyChildren();
		}

		// Same as DestroyChildren, but does so immediately
        public static void DestroyChildrenImmediately(this Transform tform)
        {
            for (int i = tform.childCount - 1; i >= 0; i--)
                GameObject.DestroyImmediate(tform.GetChild(i).gameObject);
        }

		// Throws a NRE if this object is null, with an optional message
		// Usage: Camera c = GetComponent<Camera>(); c.ThrowIfNull();
        public static void ThrowIfNull(this object o, string msg = "NullReferenceException")
        {
            if (o == null) throw new System.NullReferenceException(msg);
        }
    }
}