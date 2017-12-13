using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Spewnity
{
    /**
        Triggers the loading of another scene when certain conditions are met:

        Awake: Triggers during GameObject.Awake()
        Start: Triggers during GameObject.Start()
        TriggerEnter2D: Triggers when an collidable object enters the collider's bounding volume. Collider2D required.
        MouseDown: Triggers when the mouse is pressed while pointing inside the collider's bounding volume. Collider2D required.
        KeyDown: Triggers when a key is pressed; define one or more keys in keyCodes.

        You can also play a sound effect, load a scene additively or singly, load a specific scene or the next in the list,
        load always or only if a specific GameObject is not found, trigger always or only if the colliding object has a
        specific tag, forward string data, and optionally invoke events before the scene is finally loaded.
     */
    public class SceneLoader : MonoBehaviour
    {
        public static string data; // If a trigger had SceneInfo.data, it will get written here when triggered
        public SceneInfo[] sceneInfo; // All possible triggering events here!

        private bool checkKeys = false;

        public void Awake()
        {
            foreach (SceneInfo info in sceneInfo)
                if (info.trigger == SceneTrigger.KeyDown)
                {
                    if (info.keyCodes.Length == 0)
                        Debug.Log("Warning: Trigger method is KeyDown but no keyCodes specified.");
                    checkKeys = true;
                    break;
                }
            else if (info.trigger == SceneTrigger.TriggerEnter2D || info.trigger == SceneTrigger.MouseDown)
            {
                if (GetComponent<Collider2D>() == null)
                    Debug.Log("Warning: Trigger method is " + info.trigger + " but Collider2D not present.");
            }

            CheckTrigger(SceneTrigger.Awake);
        }

        public void Start()
        {
            CheckTrigger(SceneTrigger.Start);
        }

        void Update()
        {
            if (!checkKeys)
                return;

            CheckTrigger(SceneTrigger.KeyDown);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            CheckTrigger(SceneTrigger.TriggerEnter2D, other.tag);
        }

        void OnMouseDown()
        {
            CheckTrigger(SceneTrigger.MouseDown);
        }

        private void CheckTrigger(SceneTrigger trigger, string tag = "")
        {
            foreach (SceneInfo info in sceneInfo)
            {
                // Check for trigger, optional object marker, and optional collision object tag
                if ((info.trigger == trigger) &&
                    (info.marker.IsEmpty() || transform.Find(info.marker) == null) &&
                    (trigger != SceneTrigger.TriggerEnter2D || info.tag == tag))
                {
                    // Check for keypresses
                    if (trigger == SceneTrigger.KeyDown)
                    {
                        bool keyIsDown = false;
                        foreach (KeyCode keyCode in info.keyCodes)
                        {
                            if (Input.GetKeyDown(keyCode))
                                keyIsDown = true;
                            break;
                        }
                        if (keyIsDown)
                            LoadScene(info);
                    }

                    // Success, load scene
                    else LoadScene(info);
                }
            }
        }

        private void LoadScene(SceneInfo info)
        {
            SceneLoader.data = info.data;
            info.preTrigger.Invoke();
            if (info.soundEffect != null)
            {
                GameObject go = new GameObject();
                go.SetActive(false);
                go.name = "scene loader sound holder";
                AudioSource source = go.AddComponent<AudioSource>();
                DontDestroyOnLoad(go);                
                AutoDestroy ad = go.AddComponent<AutoDestroy>();
                ad.afterSeconds = info.soundEffect.length + 0.1f;                
                source.clip = info.soundEffect;
                go.SetActive(true);
                source.Play();
            }
            if (info.name.IsEmpty())
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, info.mode);
            else SceneManager.LoadScene(info.name, info.mode);
        }
    }

    [System.Serializable]
    public struct SceneInfo
    {
        [Tooltip("When is the scene load triggered?")]
        public SceneTrigger trigger;

        [Tooltip("The name of the scene, which must also be in the Build Settings scene list; if blank, loads the next scene")]
        public string name;

        [Tooltip("Single removes all non-DontDestroyOnLoad objects, Additive does not remove anything")]
        public LoadSceneMode mode;

        [Tooltip("Will only load this scene when an object is NOT FOUND at this path (e.g. /MyObject); ignored if blank")]
        public string marker;

        [Tooltip("If the trigger is TriggerEnter2D, you can specify a tag to limit which objects can trigger it")]
        public string tag;

        [Tooltip("You can specify a SoundManager sound name here, which will be played when the trigger occurs")]
        public AudioClip soundEffect;

        [Tooltip("This string data will be stored in SceneLoader.data when the trigger occurs")]
        public string data;

        [Tooltip("If the trigger is KeyDown, specify the triggering key codes here")]
        public KeyCode[] keyCodes;

        [Tooltip("This event is called when the trigger occurs, but before loading the scene")]
        public UnityEvent preTrigger;

    }

    public enum SceneTrigger
    {
        Awake,
        Start,
        TriggerEnter2D,
        MouseDown,
        KeyDown
    }
}