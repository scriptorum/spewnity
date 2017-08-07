using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spewnity
{
    /**
     */
    public class CameraManager : MonoBehaviour
    {
        [Tooltip("If you have only one CameraManager instance, you can access it through CameraManager.instance")]
        public static CameraManager instance;
        [Tooltip("When shaking the camera, the maximum amount of +/- offset applied to the xy position")]
        public float shakeStrength = 0.1f;
        [Tooltip("When shaking the camera, the duration of the effect in seconds")]
        public float shakeDuration = 0.25f;
        [Tooltip("When shaking the camera, whether to fade out the strength of the effect over the duration or not")]
        public bool shakeFadeOut = true;
        [Tooltip("Lets you preview the camera shake effect, when debugging in the editor, in play mode; just click")]
        public bool previewShake = false;
        [Tooltip("If true, assumes Camera.main is the camera; otherwise expects this component is attached to a Camera object")]
        public bool useCameraMain = false;

        private Transform followTarget;
        private float curShakeStrength;
        private float curShakeDuration;
        private bool fadingShake;
        private float shakeTimeRemaining = 0;
        private Vector3 camCenter;

        void Awake()
        {
            instance = this;
            if (!useCameraMain && GetComponent<Camera>() == null)
                throw new UnityException("Since useCameraMain is disabled, CameraManager must be attached to the camera GameObject");
        }

        void Update()
        {
#if UNITY_EDITOR			
            if (previewShake)
            {
                if (Application.isPlaying)
                    Shake();
                else Debug.Log("Preview Shake only works in play mode");
                previewShake = false;
            }
#endif			

            if (shakeTimeRemaining <= 0f && followTarget == null)
                return;

            UpdateCamCenter();
            Vector2 shake = GetShakeOffset();
            Transform camTransform = (useCameraMain ? Camera.main.transform : transform);
            camTransform.position = camCenter + (Vector3) shake;
        }

        private void UpdateCamCenter()
        {
            if (followTarget != null)
                camCenter = new Vector3(followTarget.position.x, followTarget.position.y,
                    (useCameraMain ? Camera.main.transform.position.z : transform.position.z));
        }

        private Vector2 GetShakeOffset()
        {
            if (shakeTimeRemaining > 0)
            {
                shakeTimeRemaining -= Time.deltaTime;
                if (shakeTimeRemaining < 0)
                    shakeTimeRemaining = 0f;
                else
                {
                    float str = fadingShake ? curShakeStrength * shakeTimeRemaining / curShakeDuration : curShakeStrength;
                    float x = Random.Range(-str, str);
                    float y = Random.Range(-str, str);
                    return new Vector2(x, y);
                }
            }

            return Vector2.zero;
        }

        // Starts the camera shake as defined by the object properties
        public void Shake()
        {
            Shake(shakeStrength, shakeDuration, shakeFadeOut);
        }

        // Starts the camera shake as defined by ad-hoc properties
        public void Shake(float shakeStrength, float shakeDuration, bool fadeStrength = true)
        {
            curShakeStrength = shakeStrength;
            curShakeDuration = shakeTimeRemaining = shakeDuration;
            fadingShake = fadeStrength;
            camCenter = (useCameraMain ? Camera.main.transform.position : transform.position);
        }

        public void SetFollowTarget(Transform target)
        {
            this.followTarget = target;
        }

        public void ClearFollowTarget()
        {
            this.followTarget = null;
        }
    }
}