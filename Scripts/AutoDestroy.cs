using UnityEngine;
using System.Collections;

/**
 * Immediately destroys game object upon creation. Attach this to guide objects.
 * Use this on a folder, in combination with the EditorOnly tag, to keep prefab instances for testing.
 * They'll be excluded from final builds because of the tag, and destroyed instantly in editor playback 
 * mode if you haven't unticked the enabled box.
 * 
 * The object will be destroyed before Start(), so to be useful as a guide, make a copy of the
 * transform's position in another script's Awake() method.
 */
namespace Spewnity
{
	public class AutoDestroy : MonoBehaviour
	{
		void Awake()
		{
			Destroy(gameObject);
		}

		void OnValidate()
		{
			if(gameObject.tag != "EditorOnly")
				Debug.Log("AutoDestroy Object " + gameObject.name + " should be tagged EditorOnly");
		}
	}
}