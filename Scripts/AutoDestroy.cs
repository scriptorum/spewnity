using UnityEngine;
using System.Collections;

namespace Spewnity
{
	//
	// Immediately destroys game object upon creation.
	// Use this on a folder, in combination with the EditorOnly tag, to keep prefab instances for testing.
	// They'll be excluded from final builds because of the tag, and destroyed instantly in editor playback 
	// mode if you haven't unticked the enabled box.
	//
	public class AutoDestroy : MonoBehaviour
	{
		void Awake()
		{
			Destroy(gameObject);
		}
	}
}