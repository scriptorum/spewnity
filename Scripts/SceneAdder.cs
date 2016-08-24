using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spewnity
{
	public class SceneAdder : MonoBehaviour
	{
		public string[] sceneNames;

		public void Awake()
		{
			foreach(string sceneName in sceneNames)
				SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
		}
	}
}