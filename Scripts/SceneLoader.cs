using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spewnity
{
    /**
        Triggers the loading of another scene when certain conditions are met.
     */
    public class SceneLoader : MonoBehaviour
    {
        public SceneInfo[] sceneInfo;

        public void Awake()
        {
            CheckTrigger(SceneTrigger.Awake);
        }

        public void Start()
        {
            CheckTrigger(SceneTrigger.Start);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            CheckTrigger(SceneTrigger.OnTriggerEnter2D, other.tag);
        }

        private void CheckTrigger(SceneTrigger trigger, string tag = "")
        {
            foreach(SceneInfo info in sceneInfo)
            {
                if ((info.trigger == trigger) &&
                    (info.marker.IsEmpty() || transform.Find(info.marker) == null) &&
                    (trigger != SceneTrigger.OnTriggerEnter2D || info.tag == tag))
                {
                    if (info.name.IsEmpty())
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                    else SceneManager.LoadScene(info.name, info.mode);
                }
            }
        }
    }

    [System.Serializable]
    public struct SceneInfo
    {
        [Tooltip("The name of the scene, which must also be in the Build Settings scene list; if blank, loads the next scene")]
        public string name;

        [Tooltip("Single removes all non-DontDestroyOnLoad objects, Additive does not remove anything")]
        public LoadSceneMode mode;

        [Tooltip("Will only load this scene when an object is NOT FOUND at this path (e.g. /MyObject); ignored if blank")]
        public string marker;

        [Tooltip("When is the scene load triggered?")]
        public SceneTrigger trigger;

        [Tooltip("If the trigger is OnTriggerEnter2D, you can specify a tag to limit which objects can trigger it")]
        public string tag;
    }

    public enum SceneTrigger
    {
        Awake,
        Start,
        OnTriggerEnter2D
    }
}