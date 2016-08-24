/**
 * Transitions to a new scene due to a button click.
 * To use, place a collider2D on the GameObject serving as the button.
 * Define the name of the scene to transition to, and any keystrokes that should cause the transition.
 * When the transition occurs, an optional click sound may play. Additionally, callbacks may be
 * associated before the new scene is loaded.
 * */
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Events;

namespace Spewnity
{
	public class TransitionToScene : MonoBehaviour
	{
		public string clickSoundName = "";
		public string sceneName;
		public KeyCode[] keys;
		public LoadSceneMode mode = LoadSceneMode.Single;
		public UnityEvent clickEvent;

		void Update()
		{
			foreach(KeyCode key in keys)
			{
				if(Input.GetKeyDown(key))
				{
					transition();
					break;
				}
			}
		}

		public void OnMouseDown()
		{
			transition();
		}

		public void transition()
		{
			if(clickSoundName != null && clickSoundName != "" && SoundManager.instance != null)
				SoundManager.instance.Play(clickSoundName);

			clickEvent.Invoke();

			SceneManager.LoadScene(sceneName, mode);
		}
	}
}