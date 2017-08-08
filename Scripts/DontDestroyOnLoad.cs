using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spewnity
{
	/**
		When attached to a GameObject, it will be marked as DontDestroyOnLoad. This means it will survive
		transitions when loading new scenes non-additively.
	 */
    public class DontDestroyOnLoad : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}