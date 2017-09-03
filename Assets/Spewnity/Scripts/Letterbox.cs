/**
 *  Resizes the camera's viewport to maintain the desired aspect ratio without clipping.
 *  Adds letter/pillar-boxing black bars as needed. Can attach this to the Camera.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letterbox : MonoBehaviour
{
	[Tooltip("Aspect ratio or desired screen resolution")]
	public Vector2 aspectRatio = new Vector2(16, 10);
	[Tooltip("How frequently to recheck window size change, in seconds")]
	public float recheckInterval = 1.0f;
	[Tooltip("The camera to show behind the main camera, usually showing letterboxing bars. Optional.")]
	public Camera backingCamera;

	void OnValidate()
	{
	}

	void Start()
	{
		if(backingCamera == null)
		{
			GameObject go = new GameObject("CameraSizer Backing Camera");
			go.transform.parent = transform;
			backingCamera = go.AddComponent<Camera>();
			backingCamera.clearFlags = CameraClearFlags.SolidColor;
			backingCamera.backgroundColor = Color.black;
			backingCamera.depth = Camera.main.depth - 1;
			backingCamera.cullingMask = 0;
		}
		InvokeRepeating("checkAspect", 0f, recheckInterval);
	}

	public void checkAspect()
	{		
		float actualAspect = (float) Screen.width / Screen.height;
		float targetAspect = aspectRatio.x / aspectRatio.y;
		if(targetAspect == actualAspect) return;

		Rect rect = Camera.main.rect;
		if(actualAspect < targetAspect)
		{
			// Letterbox
			float scale = actualAspect / targetAspect;
			rect.Set(0f, (1.0f - scale) / 2.0f, 1.0f, scale);
		}
		else
		{
			// Pillar box
			float scale = targetAspect / actualAspect;
			rect.Set((1.0f - scale) / 2.0f, 0f, scale, 1.0f);
		}
		Camera.main.rect = rect;
	}
}

