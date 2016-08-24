﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading;
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

		// Processes all actions in the queue, or until an action causes the queue to pause,
		// in which case the queue will resume itself when it's ready.
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

		public int Count()
		{
			return actions.Count;
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
		// If the non-null, this may also "select" the game object.
		private GameObject Qualify(GameObject go, bool alsoSelectIt = true)
		{
			if(go != null)
			{
				if(alsoSelectIt) selectedGameObject = go;
				return go;
			}

			if(selectedGameObject == null) throw new UnityException("Action requires a GameObject, but none was supplied or selected");
			
			return selectedGameObject;			
		}

		// Tries to cancel the currently playing event
		// Leaves the action queue unpaused but not running, call Run() or Resume() to continue processing queue.
		public void Cancel()
		{
			StopAllCoroutines();
			CancelInvoke();
			this.paused = false;
		}

		/////////// SOME FUNCTIONS THAT ADD PRE-DEFINED ACTIONS

		// Pauses the queue for delaySec and the resumes processing.
		// This action can be cancelled. See Cancel().
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

		public ActionQueue RemoveComponent<T>(GameObject go = null) where T:Component
		{
			Add(() => GameObject.Destroy(Qualify(go).GetComponent<T>()));
			return this;
		}

		// Plays a sound and optionally waits for it to finish
		public ActionQueue PlaySound(string soundName, bool waitForIt = false)
		{
			Add(() =>
			{
				if(waitForIt) 
				{
					Pause();
					SoundManager.instance.Play(soundName, (snd) => Resume());
				}
				else SoundManager.instance.Play(soundName);
			});
			return this;
		}

		public ActionQueue InvokeEvent(UnityEvent evt)
		{
			Add(() => evt.Invoke());
			return this;
		}

		// Instantiates a game object and selects it (see Select)
		public ActionQueue Instatiate(GameObject prefab = null)
		{
			Add(() => GameObject.Instantiate(Qualify(prefab, true)));
			return this;
		}

		// Instantiates a game object at a certain position and selects it (see Select)
		public ActionQueue Instantiate(Vector3 position, GameObject prefab = null)
		{
			Add(() => GameObject.Instantiate(Qualify(prefab, true), (Vector3) position, Quaternion.identity));
			return this;
		}

		// Destroys the game object and also deselects it
		public ActionQueue Destroy(GameObject go = null)
		{
			Add(() =>
			{
				GameObject.Destroy(Qualify(go));
				selectedGameObject = null;
			});
			return this;
		}

		// Sets the selected game object's parent
		public ActionQueue SetParent(GameObject parent)
		{
			Add(() => Qualify(null).transform.parent = parent.transform);
			return this;
		}

		// Selects a game object. You can pass null for any these predefined actions
		// and it will use the selected game object instead. Some actions will
		// automatically select a game object.
		public ActionQueue Select(GameObject go)
		{
			Add(() => selectedGameObject = go);
			return this;
		}

		public ActionQueue Log(string msg)
		{
			Add(() => Debug.Log(msg));
			return this;
		}
	}
}

