using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO Rework exposed interface - for example, maybe someone would want to set target in the editor?
namespace Spewnity
{
    /// <summary>
    /// CameraDirector provides several functions to ease 2D camera management.
    /// <para>Camera shaking, follow a moving target, cut to or dolly to a specific position, 
    /// or zoom to a specific camera size.</para>
    /// <para>Note all dolly/follow animation is done using a non-linear continuous ease-out curve, whereby it will reach 
    /// approximately 99% of the target within the duration indicated, and 90% of the target within half the duration.</para>
    /// <para>Zooming manipulates the camera's orthographicSize or fieldOfView. You may want to set up a second
    /// camera to show your HUD, so that camera is not scaled. To do this: create a new Camera for your HUD, set
    /// clearFlags to DepthOnly, give it a depth higher than your main camera (say, 1). Put all your HUD GameObjects
    /// on their own layer (e.g., UI), and set the HUD camera's culling mask to just render UI. Set your main camera
    /// to render everything but UI using the culling mask.
    /// </summary>
    public class CameraDirector : MonoBehaviour
    {
        [Tooltip("If you have only one CameraDirector instance, you can access it through CameraDirector.instance")]
        public static CameraDirector instance;
        [Tooltip("When shaking the camera, the maximum amount of +/- offset applied to the xy position")]
        public float defShakeStrength = 0.1f;
        [Tooltip("When shaking the camera, the duration of the effect in seconds")]
        public float defShakeDuration = 0.25f;
        [Tooltip("When following, dollying or zooming, the amount of time it takes to reach the target")]
        public float defSpeed = 1f;
        [Tooltip("When shaking the camera, whether to fade out the strength of the effect over the duration or not")]
        public bool shakeFadeOut = true;
        [Tooltip("Lets you preview the camera shake effect, when debugging in the editor, in play mode; just click")]
        public bool previewShake = false;
        [Tooltip("Assigns the camera to use for this instance; leave blank to use Camera.main")]
        public Camera cam;

        private const float CONTINUOUS_EASING = 4.6f; // Gets us to 99% of move target over duration
        private Transform followTarget;
        private float shakeStrength;
        private float shakeDuration;
        private float moveSpeed;
        private bool fadingShake = false;
        private bool dollying = false;
        private float shakeTimeRemaining = 0f;
        private Vector3 shakeCenter;
        private float startZoom;
        private float targetZoom;
        private float zoomTimeRemaining = 0f;
        private float zoomSpeed;
        private float rotTimeRemaining = 0f;
        private float targetRot;
        private float rotSpeed;
        private float startRot;

        void Awake()
        {
            instance = this;
            Init();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Init();
        }
#endif

        private void Init()
        {
            if (cam == null)
                cam = Camera.main;
            startZoom = cam.orthographic ? cam.orthographicSize : cam.fieldOfView;
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

            UpdateRotation();
            UpdateZoomLevel();

            if (shakeTimeRemaining <= 0f && followTarget == null && dollying == false)
                return;

            UpdateCamCenter();
            Vector2 shake = GetShakeOffset();
            if (!dollying && moveSpeed <= 0f)
                cam.transform.position = shakeCenter + (Vector3) shake;
            else
            {
                float t = CONTINUOUS_EASING * Time.deltaTime / moveSpeed;
                cam.transform.position = Vector3.Lerp(cam.transform.position, shakeCenter, t) + (Vector3) shake;
            }
        }

        private void UpdateZoomLevel()
        {
            if (zoomTimeRemaining <= 0f)
                return;
            zoomTimeRemaining -= Time.deltaTime;

            float amount = 0;
            if (zoomTimeRemaining < 0)
            {
                zoomTimeRemaining = 0;
                amount = targetZoom;
            }
            else
            {
                float t = 1 - zoomTimeRemaining / zoomSpeed;
                amount = Mathf.Lerp((cam.orthographic ? cam.orthographicSize : cam.fieldOfView), targetZoom, t);
            }

            if (cam.orthographic)
                cam.orthographicSize = amount;
            else cam.fieldOfView = amount;
        }

        private void UpdateRotation()
        {
            if (rotTimeRemaining <= 0f)
                return;
            rotTimeRemaining -= Time.deltaTime;

            float amount = 0;
            if (rotTimeRemaining < 0)
            {
                rotTimeRemaining = 0;
                amount = targetRot;
            }
            else
            {
                float t = 1 - rotTimeRemaining / rotSpeed;
                amount = Mathf.Lerp(startRot, targetRot, t);
            }

            cam.transform.localRotation = Quaternion.Euler(0, 0, amount);
        }

        private void UpdateCamCenter()
        {
            if (followTarget != null)
                shakeCenter = new Vector3(followTarget.position.x, followTarget.position.y, cam.transform.position.z);
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

        /// <summary>
        /// Starts the camera shake as defined by the default properties. 
        /// <see cref="Shake(float,float,bool)"/>
        /// </summary>
        public CameraDirector Shake()
        {
            Shake(defShakeStrength, defShakeDuration, shakeFadeOut);
            return this;
        }

        /// <summary>
        /// Starts the camera shake as defined by ad-hoc properties. 
        /// <para>You can click on the Preview Shake checkbox to sample the shake while running in the editor.</para>
        /// <para>To stop the camera shaking, call StopShaking()</para>
        /// </summary>
        /// <param name="shakeStrength">How far the camera can veer off-center +/-. </param>
        /// <param name="shakeDuration">How long the shaking lasts.</param>
        /// <param name="fadeStrength">If true, the strength is linearly reduced over the time of the shaking.</param>
        public CameraDirector Shake(float shakeStrength, float shakeDuration, bool fadeStrength = true)
        {
            this.shakeStrength = shakeStrength;
            this.shakeDuration = shakeTimeRemaining = shakeDuration;
            fadingShake = fadeStrength;
            shakeCenter = cam.transform.position;
            return this;
        }

        /// <summary>
        /// Stops any dollying or following of a target. Clears the target.
        /// <para>Does not stop shaking or zooming</para>
        /// </summary>
        public CameraDirector StopMoving()
        {
            followTarget = null;
            dollying = false;
            return this;
        }

        /// <summary>
        /// Stops all camera shaking
        /// </summary>
        public CameraDirector StopShaking()
        {
            shakeTimeRemaining = 0f;
            return this;
        }

        /// <summary>
        /// Stops any active zooming operation.
        /// </summary>
        public CameraDirector StopZooming()
        {
            zoomTimeRemaining = 0f;
            return this;
        }

        public CameraDirector StopRotating()
        {
            rotTimeRemaining = 0f;
            return this;
        }

        public CameraDirector Stop()
        {
            StopMoving();
            StopRotating();
            StopShaking();
            StopZooming();
            return this;
        }

        /// <summary>
        /// Resets the zoom level to its initial value.
        /// </summary>
        public CameraDirector ResetZoom()
        {
            SetZoom(1f);
            return this;
        }

        /// <summary>
        /// Sets the camera to follow a target.
        /// <para>This also stops dollying</para>
        /// </summary>
        /// <param name="target">The Transform of the GameObject to follow; the camera will center over this object</param>
        /// <param name="speed">The amount of time the camera will take to catch up to the target. Supply 0 if you don't want any camera lag.</param>
        public CameraDirector FollowTarget(Transform target, float? speed = null)
        {
            StopMoving();
            this.followTarget = target;
            this.moveSpeed = (speed == null ? this.defSpeed : (float) speed);
            return this;
        }

        /// <summary>
        /// Jumps the camera to a specific position, immediately.
        /// <para>If the CameraDirector was dollying or following a target, it stops doing that.</para>
        /// </summary>
        /// <param name="position">Sets the absolute center point of the camera</param>
        public CameraDirector CutTo(Vector2 position)
        {
            StopMoving();
            cam.transform.position = new Vector3(position.x, position.y, cam.transform.position.z);
            shakeCenter = cam.transform.position;
            return this;
        }

        /// <summary>
        /// Dollies the camera to a specific position over the amount of time specified by speed.
        /// <para>Note that all camera movement eases out, so it will reach 90% of the target in half the time.</para>
        /// <para>If the CameraDirector was following a target, it stops doing that.</para>
        /// </summary>
        /// <param name="position">Sets the desired absolute center point of the camera</param>
        /// <param name="speed">If not provided, uses the default move speed. Using a speed 0 will work the same as CutTo().</param>
        public CameraDirector DollyTo(Vector2 position, float? speed = null)
        {
            StopMoving();
            shakeCenter = new Vector3(position.x, position.y, cam.transform.position.z);
            dollying = true;
            this.moveSpeed = (speed == null ? this.defSpeed : (float) speed);
            return this;
        }

        /// <summary>
        /// Sets the zoom level of the camera.
        /// <para>This change to zoom level is immediate.</para>
        /// <see crf="Zoom"/> for more information
        /// </summary>
        /// <param name="scale">The amount to scale orthographic size or field-of-view. 1 is none.</param>
        public CameraDirector SetZoom(float scale)
        {
            StopZooming();
            float amount = startZoom * scale;
            if (cam.orthographic)
                cam.orthographicSize = amount;
            else cam.fieldOfView = amount;
            return this;
        }

        /// <summary>
        /// Begins to zoom in or out to the desired zoom scale, over time.
        /// <para>This works by setting the orthographic size or field-of-view of the camera.</para>
        /// <see crf="ResetZoom"/>
        /// <see crf="StopZoom"/>
        /// </summary>
        /// <param name="scale">The amount to scale orthographic size or field-of-view. A value of 2 is zoomed out to show double the view, and 0.5 zoomed in to halve the view. 1 resets to the initial size. </param>
        /// <param name="speed">The amount of time before it reaches the target scale.</param>
        public CameraDirector ZoomTo(float scale, float? speed = null)
        {
            zoomSpeed = zoomTimeRemaining = (speed == null ? this.defSpeed : (float) speed);
            targetZoom = startZoom * scale;
            return this;
        }

        public CameraDirector SetRotation(float angle)
        {
            StopRotating();
            cam.transform.localRotation = Quaternion.Euler(0, 0, angle);
            return this;
        }

        public CameraDirector RotateTo(float angle, float? speed = null)
        {
            startRot = cam.transform.localEulerAngles.z;
            rotSpeed = rotTimeRemaining = (speed == null ? this.defSpeed : (float)speed);
            targetRot = ShortestRotation(startRot, angle);
            return this;
        }

        private static float ShortestRotation(float from, float to)
        {
            float diff = from - to;
            return (diff < -180 ? to + 360 : (diff > 180 ? to - 360 : to));
        }
    }
}