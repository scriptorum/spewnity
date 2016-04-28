using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Action = System.Action;

namespace Spewnity
{
	public class ActionQueue: MonoBehaviour
	{
		public List<Action> actions = new List<Action>();
		public bool paused = false;

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

		/////////// SOME FUNCTIONS THAT ADD PRE-DEFINED ACTIONS

		public ActionQueue Delay(float delaySec)
		{
			Add(() =>
			{
				this.Pause();
				Invoke("Resume", delaySec);
			});
			return this;
		}

		public ActionQueue AddComponent<T>(GameObject go) where T:Component
		{
			Add(() => go.AddComponent<T>());
			return this;
		}

		public ActionQueue PlaySound(string soundName)
		{
			Add(() =>
			{
				Pause();
				SoundManager.instance.play(soundName, (snd) => Resume());
			});
			return this;
		}
	}
}

