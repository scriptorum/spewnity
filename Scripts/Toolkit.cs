﻿//TODO Add StartCoroutine versions of the Lerps? I'll have to extend them off MonoBehaviour then.
using UnityEngine;
using System.Collections;

namespace Spewnity
{
	public static class Toolkit
	{
		// Tweens the transform from its current position to endPos in world space.
		// Remember to wrap in a StarCoroutine().
		// Action triggered at end of tween. Example Action: (t) => Debug.Log("Transform complete!")
		public static IEnumerator LerpPosition(this Transform tform, Vector3 endPos, float duration, 
			AnimationCurve curve = null, System.Action<Transform> onComplete = null)
		{
			Vector3 startPos = tform.position;
		
			float t = 0;
			while(t < 1)
			{
				yield return null;
				t += Time.deltaTime / duration;
				tform.position = Vector3.Lerp(startPos, endPos, (curve == null ? t : curve.Evaluate(t)));
			}
		
			if(onComplete != null)
				onComplete.Invoke(tform);
		}

		// Tweens the transform from its current scale to endScale in world space.
		// Remember to wrap in a StarCoroutine().
		// Action triggered at end of tween. Example Action: (t) => Debug.Log("Transform complete!")
		public static IEnumerator LerpScale(this Transform tform, Vector3 endScale, float duration, 
			AnimationCurve curve = null, System.Action<Transform> onComplete = null)
		{
			Vector3 startScale = tform.localScale;
		
			float t = 0;
			while(t < 1)
			{
				yield return null;
				t += Time.deltaTime / duration;
				tform.localScale = Vector3.Lerp(startScale, endScale, (curve == null ? t : curve.Evaluate(t)));
			}
		
			if(onComplete != null)
				onComplete.Invoke(tform);
		}

		// Tweens the between two colors. Sends the interpolated color to onUpdate().
		// Remember to wrap in a StarCoroutine().
		public static IEnumerator LerpColor(this Color startColor, Color endColor, float duration, System.Action<Color> onUpdate,
			AnimationCurve curve = null, System.Action onComplete = null)
		{
			float t = 0;
			while(t < 1)
			{
				yield return null;
				t += Time.deltaTime / duration;
				onUpdate(Color.Lerp((Color) startColor, endColor, (curve == null ? t : curve.Evaluate(t))));
			}

			if(onComplete != null)
				onComplete.Invoke();
		}

		// Tweens the between two float values. Sends the interpolated float to onUpdate().
		// Remember to wrap in a StarCoroutine().
		public static IEnumerator LerpFloat(this float startValue, float endValue, float duration, System.Action<float> onUpdate, 
			AnimationCurve curve = null,  System.Action onComplete = null)
		{
			float t = 0;
			while(t < 1)
			{
				yield return null;
				t += Time.deltaTime / duration;
				onUpdate(Mathf.Lerp(startValue, endValue, (curve == null ? t : curve.Evaluate(t))));
			}

			if(onComplete != null)
				onComplete.Invoke();
		}

		// Snaps the XY component of a Vector3 to a 45 degree angle 
		public static Vector3 Snap45(this Vector3 v3, float snapAngle)
		{
			float angle = Vector3.Angle(v3, Vector3.up);
			if(angle < snapAngle / 2.0f)          // Cannot do cross product 
				return Vector3.up * v3.magnitude;  //   with angles 0 & 180
			if(angle > 180.0f - snapAngle / 2.0f)
				return Vector3.down * v3.magnitude;
		
			float t = Mathf.Round(angle / snapAngle);
			float deltaAngle = (t * snapAngle) - angle;
		
			Vector3 axis = Vector3.Cross(Vector3.up, v3);
			Quaternion q = Quaternion.AngleAxis(deltaAngle, axis);
			return q * v3;
		}

		// Shuffles an array in place. Also returns array.
		public static T[] Shuffle<T>(this T[] arr)
		{
			for(int i = arr.Length - 1; i > 0; i--)
			{
				int r = Random.Range(0, i + 1);
				if(r != i)
				{
					T tmp = arr[i];
					arr[i] = arr[r];
					arr[r] = tmp;
				}
			}
			return arr;
		}
	
		public static string GetFullPath(this Transform o)
		{
			if(o.parent == null)
				return "/" + o.name;
			return o.parent.GetFullPath() + "/" + o.name;
		}

		public static T Rnd<T>(this T[] arr)
		{
			Debug.Assert(arr.Length > 0);
			return arr[Random.Range(0, arr.Length)];
		}

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

		// Joins an array of some type into a comma separated string
		public static string Join<T>(T[] arr, string delim = ",")
		{
			return string.Join(delim, System.Array.ConvertAll<T,string>(arr, x => x.ToString()));
		}

		public static Transform GetChild(this Transform tform, string name)
		{
			Transform child = tform.FindChild(name);
			Debug.Assert(child != null, "Could not find child " + name + " under " + tform.name);
			return child;
		}

		public static GameObject GetChild(this GameObject go, string name)
		{
			return go.transform.GetChild(name).gameObject;
		}

		public static T GetComponentOf<T>(string name)
		{
			GameObject go = GameObject.Find(name);
			Debug.Assert(go != null);
			T component = go.GetComponent<T>();
			Debug.Assert(component != null);
			return component;
		}
	}
}