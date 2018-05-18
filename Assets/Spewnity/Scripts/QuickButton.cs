using System.Collections;
using System.Collections.Generic;
using Spewnity;
using UnityEngine;
using UnityEngine.Events;


public class QuickButton : MonoBehaviour
{
	public Color clickTint = Color.grey;
	public AudioClip soundEffect;
	public UnityEvent onClick;

	private Color oldTint;
	private SpriteRenderer sr;

	void Awake()
	{
		gameObject.Assign(ref sr);
		Collider2D col = gameObject.GetComponent<Collider2D>();
		if(col == null)
			gameObject.AddComponent<PolygonCollider2D>();
	}

	void OnMouseDown()
	{
		oldTint = sr.color;
		sr.color = clickTint;

		if (soundEffect != null)
		{
			AudioSource source = gameObject.GetComponent<AudioSource>();
			if(source == null)
				source = gameObject.AddComponent<AudioSource>();
			source.clip = soundEffect;
			source.Play();
		}
	}

	void OnMouseUp()
	{
		sr.color = oldTint;
		onClick.Invoke();
	}
}