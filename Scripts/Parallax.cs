using UnityEngine;
using System.Collections;

/**
 * Parallax behavior.
 * Attach this to the object that you wish to move in a parallax manner. 
 * This object should probably be a child of the camera.
 * Measure the extents of the camera movement and the extents this object local to the camera.
 * If only wish to parallax on one axis, leave the extents for the other axis at 0.
 **/
namespace Spewnity
{
	[ExecuteInEditMode]
	public class Parallax : MonoBehaviour
	{
		[Tooltip("Min and max extents of the camera position")]
		public Vector2 minCamera, maxCamera;
		[Tooltip("Min and max extents of the this parallaxed object")]
		public Vector2 maxParallax, minParallax;
		[Tooltip("When the camera moves beyond its extents, should this object scroll with the camera 1:1")]
		public bool clampParallax = true;
		[Tooltip("Should this object parllax in reverse direction?")]
		public bool reverseParallaxX = false;
		public bool reverseParallaxY = false;

		private Vector2 lastCamera, cameraRange, parallaxRange;

		public void Awake()
		{
			Init();
		}

		public void OnValidate()
		{
			Init();
		}

		public void Init()
		{
			lastCamera.x = Camera.main.transform.position.x;
			lastCamera.y = Camera.main.transform.position.y;
			cameraRange.x = maxCamera.x - minCamera.x;
			cameraRange.y = maxCamera.y - minCamera.y;
			parallaxRange.x = maxParallax.x - minParallax.x;
			parallaxRange.y = maxParallax.y - minParallax.y;
		}

		public void Update()
		{
			Vector3 pos = transform.localPosition;
			Vector2 rangedResult, result;
			Vector2 curCam = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y);
			if(curCam.x == lastCamera.x && curCam.y == lastCamera.y) return;

			lastCamera = curCam;

			if(cameraRange.x > 0 && parallaxRange.x > 0)
			{
				rangedResult.x = parallaxRange.x * ((curCam.x - minCamera.x) / cameraRange.x);
				result.x = reverseParallaxX ? minParallax.x + rangedResult.x : maxParallax.x - rangedResult.x;
				pos.x = (clampParallax ? Mathf.Clamp(result.x, minParallax.x, maxParallax.x) : result.x);
			}

			if(cameraRange.y > 0 && parallaxRange.y > 0)
			{
				rangedResult.y = parallaxRange.y * ((curCam.y - minCamera.y) / cameraRange.y);
				result.y = reverseParallaxY ? minParallax.y + rangedResult.y : maxParallax.y - rangedResult.y;
				pos.y = (clampParallax ? Mathf.Clamp(result.y, minParallax.y, maxParallax.y) : result.y);
			}
	
			transform.localPosition = pos;
		}
	}
}