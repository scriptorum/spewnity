using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Action = System.Action;

namespace Spewnity
{
	public class ActionQueue: MonoBehaviour
	{
		public List<Action> actions = new List<Action>();
		public bool paused = false;
		public GameObject selectedGameObject = null;

		public ActionQueue Add(Action action)
		{
			actions.Add(action);
			return this;
		}

		public ActionQueue Insert(Action action)
		{
			actions.Insert(0, action);
			return this;
		}

		public void Run()
		{
			// Run until paused or out of actions
			while(!paused && actions.Count > 0)
			{
				// Run the next action
				Action action = actions[0];
				actions.RemoveAt(0);
				action();
			}
		}

		public void Resume()
		{
			this.paused = false;
			Run();
		}

		public void Pause()
		{
			this.paused = true;
		}

		public void Clear()
		{
			actions.Clear();
		}

		// Returns a qualified, non-null game object, either the supplied parameter (if non-null) or the "selected" game object.
		private GameObject Qualify(GameObject go)
		{
			if(go != null) return go;

			if(selectedGameObject == null) Debug.Log("Action references a null GameObject but selectedGameObject is also null");
			
			return selectedGameObject;			
		}

		/////////// SOME FUNCTIONS THAT ADD PRE-DEFINED ACTIONS

		public ActionQueue Delay(float delaySec)
		{
			Add(() =>
			{
				Pause();
				Invoke("Resume", delaySec);
			});
			return this;
		}

		public ActionQueue AddComponent<T>(GameObject go = null) where T:Component
		{
			Add(() => Qualify(go).AddComponent<T>());
			return this;
		}

		public ActionQueue PlaySound(string soundName)
		{
			Add(() =>
			{
				Pause();
				SoundManager.instance.Play(soundName, (snd) => Resume());
			});
			return this;
		}

		public ActionQueue InvokeEvent(UnityEvent evt)
		{
			Add(() => evt.Invoke());
			return this;
		}

		// Instantiates a game object and selects it (see Select)
		public ActionQueue Spawn(GameObject prefab = null)
		{
			Add(() => selectedGameObject = (GameObject) 
				Instantiate(Qualify(prefab)));
			return this;
		}

		// Instantiates a game object at a certain position and selects it (see Select)
		public ActionQueue SpawnAt(Vector3 position, GameObject prefab = null)
		{
			Add(() => selectedGameObject = (GameObject) 
				Instantiate(Qualify(prefab), (Vector3) position, Quaternion.identity));
			return this;
		}

		// Selects a game object. You can pass null for any these predefined actions
		// and it will use the selected game object instead.
		public ActionQueue Select(GameObject go)
		{
			Add(() => selectedGameObject = go);
			return this;
		}
	}
}

