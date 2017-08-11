﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
    This provides a simple way to offload coroutine ownership to another GameObject.
    Simply attach this component to a new GameObject, and run coroutines with
    CoroutineManager.instance.Run(YourCoroutineFunc());
 */
namespace Spewnity
{
    public class CoroutineManager : MonoBehaviour
    {
        public static CoroutineManager instance;

        void Awake()
        {
            instance = this;
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
    }
}