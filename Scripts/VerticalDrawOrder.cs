using UnityEngine;
using System.Collections;

namespace Spewnity
{
	// When assigned to a set of game objects, ensures that
	// the lower objects are positioned in front of higher objects.
	// For simulating depth in a 2D game.
	//
	// This can be done either by manipulating the Z position,
	// or the SpriteRenderer sort order.
	public class VerticalDrawOrder : MonoBehaviour
	{
		private SpriteRenderer sr;
		//	private float amount = 0.01f;

		void Awake()
		{
			sr = GetComponent<SpriteRenderer>();
		}

		void Update()
		{
			//		if(transform.hasChanged)
			//		{
			sr.sortingOrder = (int) (transform.position.y * -1000);
			//			transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * amount);
			//			transform.hasChanged = false; // this is competing with FlipAndFacePlayer
			//		}
		}
	}
}