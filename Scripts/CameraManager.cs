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
        public float defShakeStrength = 0.1f;
        [Tooltip("When shaking the camera, the duration of the effect in seconds")]
        public float defShakeDuration = 0.25f;
        [Tooltip("When following a target or dollying, if nonzero, the amount of time the camera lag behinds the target")]
        public float defCameraLag = 1f; // The lag will ease-out, so it gets within 90% of the target in half the lag time
        [Tooltip("When shaking the camera, whether to fade out the strength of the effect over the duration or not")]
        public bool shakeFadeOut = true;
        [Tooltip("Lets you preview the camera shake effect, when debugging in the editor, in play mode; just click")]
        public bool previewShake = false;
        [Tooltip("If true, assumes Camera.main is the camera; otherwise expects this component is attached to a Camera object")]
        public bool useCameraMain = false;

        private Transform followTarget;
        private float shakeStrength;
        private float shakeDuration;
        private float cameraLag;
        private bool fadingShake = false;
        private bool dollying = false;
        private float shakeTimeRemaining = 0f;
        private Vector3 camCenter;

        void Awake()
        {
            instance = this;
            if (!useCameraMain && GetComponent<Camera>() == null)
                throw new UnityException("Since useCameraMain is disabled, CameraManager must be attached to the camera GameObject");
        }

        void FixedUpdate()
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

            if (shakeTimeRemaining <= 0f && followTarget == null && dollying == false)
                return;

            UpdateCamCenter();
            Vector2 shake = GetShakeOffset();
            Transform camTransform = (useCameraMain ? Camera.main.transform : transform); // TODO Cache this
            if (!dollying && cameraLag <= 0f)
                camTransform.position = camCenter + (Vector3) shake;
            else
            {
                // 4.5 - This magic number gets us to about 99% of the target after the desired number of frames.
                // Do I understand the math? Hell no. But this was the shit I was entering in Wolfram Alpha:
                //      a=b*v, b=c*v, c=d*v,d=v
                //      v=(g(n) - g(1 + n))/(g(n) - 100) where 100 is a target value.
                camTransform.position = Vector3.Lerp(camTransform.position, camCenter,
                    4.5f * Time.deltaTime / cameraLag) + (Vector3) shake;
            }
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
                    float str = fadingShake ? shakeStrength * shakeTimeRemaining / shakeDuration : shakeStrength;
                    float x = Random.Range(-str, str);
                    float y = Random.Range(-str, str);
                    return new Vector2(x, y);
                }
            }

            return Vector2.zero;
        }

        // Starts the camera shake as defined by the default properties.
        public void Shake()
        {
            Shake(defShakeStrength, defShakeDuration, shakeFadeOut);
        }

        // Starts the camera shake as defined by ad-hoc properties.
        // Strength is how far the camera can veer off-center +/-. 
        // Duration is how long the shaking lasts.
        // If fadeStrength is true, the strength is linearly reduced over the time of the shaking.
        public void Shake(float shakeStrength, float shakeDuration, bool fadeStrength = true)
        {
            this.shakeStrength = shakeStrength;
            this.shakeDuration = shakeTimeRemaining = shakeDuration;
            fadingShake = fadeStrength;
            camCenter = (useCameraMain ? Camera.main.transform.position : transform.position);
        }

        public void Clear()
        {
            this.followTarget = null;
            dollying = false;
        }

        // Sets the camera to follow a target.
        // If lag is not provided, uses the default lag. Supply 0f if you want instant following.
        // If the CameraManager was dollying, it stops doing that.
        public void FollowTarget(Transform target, float? cameraLag = null)
        {
            Clear();
            this.followTarget = target;
            this.cameraLag = (cameraLag == null ? this.defCameraLag : (float) cameraLag);
        }

        // Jumps the camera to a specific position, immediately.
        // If the CameraManager was dollying or following, it stops doing that.
        public void CutTo(Vector2 position)
        {
            Clear();
            Transform camTransform = (useCameraMain ? Camera.main.transform : transform); // TODO Cache this
            camTransform.position = new Vector3(position.x, position.y, camTransform.position.z);
            camCenter = camTransform.position;
        }

        // Dollies the camera to a specific position, using the speed specified by lag.
        // Note that all camera movement eases out, so it will reach 90% of the target in half the lag time.
        // If lag is not provided, uses the default lag. Using a lag of 0f will work the same as CutTo.
        // If the CameraManager was following, it stops doing that.
        public void DollyTo(Vector2 position, float? cameraLag = null)
        {
            Clear();
            Transform camTransform = (useCameraMain ? Camera.main.transform : transform); // TODO Cache this
            camCenter = new Vector3(position.x, position.y, camTransform.position.z);
            dollying = true;
            this.cameraLag = (cameraLag == null ? this.defCameraLag : (float) cameraLag);
        }
    }
}