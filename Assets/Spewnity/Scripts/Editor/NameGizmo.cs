using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Spewnity
{
	public class NameGizmo : MonoBehaviour
	{
		void OnDrawGizmos()
		{
			UnityEditor.Handles.Label(transform.position, transform.name);
		}
	}
}