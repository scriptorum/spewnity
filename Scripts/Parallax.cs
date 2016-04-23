using UnityEngine;
using System.Collections;

namespace Spewnity
{
	// Camera will move beyond its extents, but the parallax will be clamped
	[ExecuteInEditMode]
	public class Parallax : MonoBehaviour
	{
		public float minCameraX = -100f;
		public float maxCameraX = 30f;
		public float minParallaxX = -6f;
		public float maxParallaxX = 6f;
		public bool clampParallax = false;
		public bool reverseParallax = false;

		private float cameraLastX, cameraRange, parallaxRange;

		public void Awake()
		{
			cameraLastX = Camera.main.transform.position.x;
			cameraRange = (maxCameraX - minCameraX);
			parallaxRange = (maxParallaxX - minParallaxX);
		}

		public void Update()
		{
			float cameraX = Camera.main.transform.position.x;
			if(cameraX == cameraLastX) return;

			cameraLastX = cameraX;
			Vector3 pos = transform.localPosition;
			float rangedResult = parallaxRange * ((cameraX - minCameraX) / cameraRange);
			float result = reverseParallax ? minParallaxX + rangedResult : maxParallaxX - rangedResult;
			if(clampParallax) pos.x = Mathf.Clamp(result, minParallaxX, maxParallaxX);
			else pos.x = result;
			transform.localPosition = pos;
		}
	}
}