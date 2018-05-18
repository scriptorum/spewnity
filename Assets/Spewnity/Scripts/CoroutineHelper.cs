using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Action = System.Action;

/**
    This provides a simple way to offload coroutine ownership to another GameObject.
    Simply attach this component to a new GameObject, and run coroutines with
    CoroutineHelper.instance.Run(YourCoroutineFunc());
 */
namespace Spewnity
{
    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;
        public static CoroutineHelper instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameObject("CoroutineHelper").AddComponent<CoroutineHelper>();
                return _instance;
            }
        }

        /**************************************************************************/

        void Awake()
        {
            _instance = this;
        }

        /// <summary>
        ///  Example: CoroutineHelper.instance.Run(() => MyFunc("MyArg"), 1f);
        /// </summary>
        public Coroutine Run(Action action, float delay)
        {
            return StartCoroutine(RunAfter(action, delay));
        }

        public Coroutine Run(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }

        public void Stop(IEnumerator routine)
        {
            StopCoroutine(routine);
        }

        public void StopAll()
        {
            StopAllCoroutines();
        }

        private IEnumerator RunAfter(Action action, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            action();
        }
    }
}