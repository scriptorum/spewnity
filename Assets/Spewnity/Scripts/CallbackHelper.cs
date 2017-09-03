using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;
#endif

// TODO Create an editor interface for viewing and setting counts
namespace Spewnity
{
    /// <summary>
    /// General functions for responding to and kicking off callbacks. 
    /// <para>Callbacks to log messages. Callbaks to keep named counts of calls. Events to kickoff callbacks on awake, start, etc.</para>
    /// </summary>
    public class CallbackHelper : MonoBehaviour
    {
        public bool logCountsNow = false;
        public CallbackHelperEvents callbackEvents;
        public List<CallbackHelperButtonEvent> buttonEvents;

        private const string DEFAULT_NAME = "default";
        private Dictionary<string, int> counter;

        void Awake()
        {
            ResetAllCounts();
            callbackEvents.Awake.Invoke();
        }

        void Start()
        {
            callbackEvents.Start.Invoke();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            callbackEvents.Validate.Invoke();

            if (logCountsNow)
            {
                logCountsNow = false;
                LogAllCounts();
            }
        }
#endif

        void Update()
        {
            callbackEvents.Update.Invoke();
        }

        /// <summary>
        /// Logs a message to the console
        /// </summary>
        /// <param name="msg">The message to log</param>
        public void Log(string msg)
        {
            Debug.Log(msg);
        }

        /// <summary>
        /// Increments the specified counter
        /// </summary>
        /// <param name="name">The name of the counter, if blank, uses "default"</param>
        public void Count(string name = null)
        {
            counter[CheckName(name)]++;
        }

        /// <summary>
        /// Returns the value of the specified counter
        /// </summary>
        /// <param name="name">The name of the counter, if blank, uses "default"</param>
        /// <returns>the value of the specified counter</returns>
        public int GetCount(string name = null)
        {
            return counter[CheckName(name)];
        }

        /// <summary>
        /// Logs the value of the specified counter to the console
        /// </summary>
        /// <param name="name">The name of the counter, if blank, uses "default"</param>
        public void LogCount(string name = null)
        {
            name = CheckName(name);
            Debug.Log(name + " counter = " + counter[name]);
        }

        /// <summary>
        /// Logs the values of all counters to the console
        /// </summary>
        public void LogAllCounts()
        {
            if (counter.Count == 0)
                Debug.Log("No counters found");

            else foreach(string key in counter.Keys) LogCount(key);
        }

        /// <summary>
        /// Resets the specified counter to zero.
        /// </summary>
        /// <param name="name">The name of the counter, if blank, uses "default"</param>
        public void ResetCount(string name = null)
        {
            counter[CheckName(name)] = 0;
        }

        /// <summary>
        /// Resets all counters to zero
        /// </summary>
        public void ResetAllCounts()
        {
            counter = new Dictionary<string, int>();
        }

        /// <summary>
        /// Internal function for providing a default name if necessary, and ensuring the 
        /// dictionary has an entry for this name.
        /// </summary>
        /// <param name="name">The name of the counter, if blank, uses "default"</param>
        /// <returns>the clarified and registered name</returns>
        private string CheckName(string name)
        {
            if (name.IsEmpty())
                name = DEFAULT_NAME;
            if (!counter.ContainsKey(name))
                counter[name] = 0;
            return name;
        }
    }

    [System.Serializable]
    public struct CallbackHelperEvents
    {
        public UnityEvent Awake;
        public UnityEvent Start;
        public UnityEvent Update;
        public UnityEvent Validate;
    }

    [System.Serializable]
    public class CallbackHelperButtonEvent
    {
        public UnityEvent Click;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof (CallbackHelperButtonEvent))]
    public class CallbackHelperButtonEventPD : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);

            if (GUI.Button(new Rect(position.width / 2f - 70f, position.yMax - 20f, 140f, EditorGUIUtility.singleLineHeight), "Activate"))
            {
                CallbackHelper helper = (CallbackHelper) property.serializedObject.targetObject;
                Match match = Regex.Match(property.propertyPath, @"\[(\d+)\]$");
                CallbackHelperButtonEvent buttonEvent = helper.buttonEvents[int.Parse(match.Groups[1].Value)];
                buttonEvent.Click.Invoke();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true) + 20f;
        }
    }
#endif
}