using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spewnity
{
    /**
        CameraManager provides several functions to ease 2D camera management.
        
         - Camera Shake. Call Shake() to shake the camera. You can supply a duration and strength, and
           whether or not you want the strength to fade out linearly over time. You can define these values
           as properties of the Camera Manager, or supply ad hoc properties in the call. Also, in run time,
           you can click on the Preview Shake checkbox to sample the shake. The camera can shake while it
           is doing other things. You can stop a shake in progress by calling StopShaking().

         - Follow Target. Call FollowTarget() to provide a Transform of the object to follow. This will
           stop any on-going dollying. If a lag is supplied, the camera will adjust its position to the target
           over time, so while the target is moving the camera will lag behind, and when it stops it will catch 
           up. Lag time eases out, such that it will reach 90% of the target in 50% of the time, and 99% of the 
           target in 100% of the time. To stop following, call Clear(), or set another target.

         - CutTo and Dolly. You can move the camera to a specific position instantly by calling CutTo(), 
           or over time by calling Dolly(). This will work while the camera is shaking, but it will stop target 
           following. The speed of the dolly follows the same principles as camera lag, defined above. To stop 
           dollying, call Clear().
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
        public float defCameraLag = 1f;
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

        // Clears target following and dollying. Does not stop shaking.
        public void Clear()
        {
            this.followTarget = null;
            dollying = false;
        }
        
        // Stops all camera shaking
        public void StopShaking()
        {
            shakeTimeRemaining = 0f;
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

        // Dollies the camera to a specific position over the amount of time specified by speed.
        // Note that all camera movement eases out, so it will reach 90% of the target in half the time.
        // If speed is not provided, uses the default camera lag. Using a speed 0f will work the same as CutTo().
        // If the CameraManager was following a target, it stops doing that.
        public void DollyTo(Vector2 position, float? speed = null)
        {
            Clear();
            Transform camTransform = (useCameraMain ? Camera.main.transform : transform); // TODO Cache this
            camCenter = new Vector3(position.x, position.y, camTransform.position.z);
            dollying = true;
            this.cameraLag = (speed == null ? this.defCameraLag : (float) speed);
        }
    }
}