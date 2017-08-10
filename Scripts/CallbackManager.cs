using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO Create an editor interface for viewing and setting counts
namespace Spewnity
{
    /// <summary>
    /// General functions for responding to callbacks. Can log messages and keep counts of calls.
    /// </summary>
    public class CallbackManager : MonoBehaviour
    {
        public bool logCountsNow = false;
        private const string DEFAULT_NAME = "default";
        private Dictionary<string, int> counter;

        void Awake()
        {
            ResetAllCounts();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (logCountsNow)
            {
                logCountsNow = false;
                LogAllCounts();
            }
        }
#endif

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
}