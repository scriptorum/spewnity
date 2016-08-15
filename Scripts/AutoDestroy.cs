using UnityEngine;
using System.Collections;

/**
 * Immediately destroys GameObject upon creation. Can optionally exclude object from final builds.  
 * Can be used to create live guides and editor guides. Can be placed on any GameObject, including
 * a folder: all child objects will affected as well.
 * 
 * Live Guide
 * A live guide is a visual GameObject that provides placement information to the game at runtime,
 * but does not appear to the player. Reference the guide in another GameObject, and during that
 * object's Awake(), make a copy of the guides's transform elements you need (e.g., position). 
 * The guide will be destroyed before Start() occurs.
 * 
 * Editor Guide
 * An editor guide is a visual GameObject that can be used to help align objects in the editor,
 * but does not appear to the player. Unlike live guides, editor guides is excluded from final
 * builds, so you cannot reference the guide's position like you can with a live guide. To make
 * this a live guide, check the excludeFromBuild box. This will tag the guide as EditorOnly.
 * 
 * To prevent accidental referencing of an editor guide, move AutoDestroy to the front of the 
 * script execution order.
 */
namespace Spewnity
{
	public class AutoDestroy : MonoBehaviour
	{
		public bool excludeFromBuild;

		void Awake()
		{
			if(excludeFromBuild) DestroyImmediate(gameObject);
			else Destroy(gameObject);
		}

		void OnValidate()
		{
			// This gives a SendMessage warning in Unity 5.5, but seems to work regardless.
			string newTag = excludeFromBuild ? "EditorOnly" : "Untagged";
			if(gameObject.tag != newTag)
				gameObject.tag = newTag;
		}
	}
}