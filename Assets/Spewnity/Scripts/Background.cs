using System;
using System.Collections;
using System.Collections.Generic;
using Spewnity;
using UnityEngine;

/**
 * Place this on any sprite. This generates a repeating or random background with parallaxing support.
 * WORK IN PROGRESS, and no I don't remember where I left off :)
 */
namespace Spewnity
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Sprite))]
	public class Background : MonoBehaviour
	{
		private SpriteRenderer sr;
		private float camWidth, camHeight;
		private int lastScreenWidth;
		private GameObjectPool pool;

		public Vector2 parallax = Vector2.one; // Optional parallax behavior

		public Camera cam;

		public void Awake()
		{
			Init();
		}

		public void OnValidate()
		{
			Init();
		}

		void Start() { }

		public void Init()
		{
			if (cam == null)
				cam = Camera.main;

			if (sr == null)
				gameObject.Assign<SpriteRenderer>(ref sr);

			if(pool == null)
			{
				pool = new GameObjectPool();
				pool.maxSize = 50;				
			}

			UpdateDimensions();
			UpdateSpritePositions();
		}

		private void UpdateDimensions()
		{
			if (lastScreenWidth == Screen.width)
				return;

			camHeight = cam.orthographicSize;
			camWidth = 2.0f * ((float) Screen.width / (float) Screen.height) * cam.orthographicSize;
			lastScreenWidth = Screen.width;
		}

		public void Update()
		{
			UpdateDimensions();
			// Vector3 pos = new Vector3(cam.transform.position.x - cam.transform.position.x * parallax.x,
			// 	cam.transform.position.y - cam.transform.position.y * parallax.y, transform.position.z);
			// UpdateSpritePositions();
			UpdateSpritePositions();
		}

		private void UpdateSpritePositions()
		{
			foreach (GameObject go in pool.available)
			{
				BackgroundTile tile = go.GetComponent<BackgroundTile>();
				go.transform.localPosition = new Vector3(tile.x * tile.size.x, tile.y * tile.size.y, 1);
			}
		}

		private void RebuildSprites()
		{
			// Release all pool sprites
			pool.ReleaseAll();

			// Disable sprite on main
			sr.enabled = false;

			// Determine how many sprites you need for scrolling
			Vector2 size = sr.bounds.size;
			int width = Mathf.CeilToInt(camWidth * 2f / size.x);
			int height = Mathf.CeilToInt(camHeight * 2f / size.y);

			// Create those sprites in the right locations
			for (int x = 0; x < width; x++)
				for (int y = 0; y < height; y++)
				{
					if (pool.Size == pool.maxSize)
						Debug.Log("Pool at max size");
					GameObject go = pool.TryGet();
					go.ThrowIfNull();
					go.transform.parent = transform;
					go.transform.rotation = transform.rotation;
					BackgroundTile tile = go.AddComponent<BackgroundTile>();
					tile.x = x;
					tile.y = y;
					tile.size = size;
				}

			// Update sprite positions
			UpdateSpritePositions();
		}
		public class BackgroundTile : MonoBehaviour
		{
			public int x;
			public int y;
			public Vector2 size;
		}
	}

}
