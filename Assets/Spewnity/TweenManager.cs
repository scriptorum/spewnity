using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

// TODO Cache TweenTemplate names in dictionary?
// TODO A Vec4 lerp is only needed for color, other TweenTypes will be more performant if you lerp just the parts you need, especially considering frozen axes
// TODO Editor and PropertyDrawer work is such a drag - look into attributes and generic helper classes?
// TODO Should Lerp be clamped for things like Colors?
// TODO Allow tweens to specify subtweens with a delay schedule
// TODO There are really only four tweenable properties for every tween - why not omit the properties array and have Position, Rotation, Scale, and Color with checkboxes next to them?
// TODO Is it possible to clone a UnityEvent? It might not be necessary but I'd like to be able to clone the TweenEvents object properly.
namespace Spewnity
{
    /// <summary>
    /// Another tweening system.
    /// <para>You can specify persistant tweens in the inspector or ad-hoc ones through the API. Persistent tweens can be run 
    /// as defined with Play(string), or they can be used as templates: Clone() and modify the tween, and then Play(tween).</para>
    /// <para>The tweens system supports 2D and 3D transform tweening, as well as color tweening on certain components.
    /// Also, freely tween independent floats, vectors, and colors using the event system. Supports easing, ping-pong, 
    /// reverse, and relative tween values.</para>
    /// <para>Preview your tweens live, just by clicking the button in the inspector while playing.</para>
    /// <para>Hook in events for tween start, change, and stop. Call Play() from Start/End to play tweens simultaneously 
    /// or in a sequence. For the latter, you can also call PlayChain(), and for the former just call Play() repeatedly.</para>
    /// </summary>
    public class TweenManager : MonoBehaviour
    {
        public static TweenManager instance;
        [Tooltip("A set of one or more persistant tweens, that can also be used as tween templates")]
        public List<Tween> tweenTemplates;
        [Tooltip("If you have multiple TweenManagers, setting this to true ensures this TweenManager is assigned to TweenManager.instance")]
        public bool primaryInstance;

        private List<Tween> tweens = new List<Tween>();
        private List<Tween> tweensToAdd = new List<Tween>();

        /// <summary>
        /// Starts tweening the object as defined by the pre-defined template tween.
        /// <para>Tween templates are defined in the inspector or by calling AddTemplate()</para>
        /// </summary>
        /// <param name="tweenName">The name of the template tween</param>
        /// <param name="go">If supplied, the tween template is cloned and the target is replaced with this GameObject</param>
        /// <param name="newName">If supplied, the tween template is cloned and given a new name; you can stop it with Stop(string)</param>
        public Tween Play(string tweenName, GameObject go = null, string newName = null)
        {
            Tween tween = GetTemplate(tweenName);
            return Play(tween, go, newName);
        }

        /// <summary>
        /// Starts tweening the object specified by the supplied tween.
        /// <para>This could be a tween obtained from the pre-defined templates, a clone of a template, or a dynamically-created Tween.</para>
        /// </summary>
        /// <param name="tween">The tween instance</param>
        /// <param name="gameObject">If supplied, the tween is cloned and the target is replaced with this GameObject</param>
        /// <param name="newName">If supplied, the tween is cloned and given a new name; you can stop it with Stop(string)</param>
        public Tween Play(Tween tween, GameObject go = null, string newName = null)
        {
            if (go != null || newName != null)
            {
                tween = tween.Clone(go);
                if (newName != null)
                    tween.name = newName;
            }

#if !TWEEN_MANAGER_SKIP_DUPE_CHECK
            if (tweens.Contains(tween) || tweensToAdd.Contains(tween))
            {
                Debug.LogError("Tween instance (" + tween.name + ") is already playing! Restarting tween.");
                tweens.Remove(tween);
                tweensToAdd.Remove(tween);
            }
#endif

            tweensToAdd.Add(tween);
            return tween;
        }

        /// <summary>
        /// Stops a tween from running by looking up its name.
        /// <para>This could be a template tween, clone, or a custom tween, as long as it has a unique name. See Stop(Tween).
        /// If there are multiple tweens with this name, all will be removed!</para>
        /// </summary>
        /// <param name="tweenName">The unique name of the tween</param>
        public void Stop(string tweenName)
        {
            foreach(List<Tween> list in new List<Tween>[] { tweens, tweensToAdd })
            {
                list.ForEach((t) => { if (t.name == tweenName) Stop(t); });
            }
        }

        /// <summary>
        /// Stops the supplied tween from running.
        /// <para>Note that the end event will not be called</para>
        /// </summary>
        /// <param name="tween">The tween instance</param>
        public void Stop(Tween tween)
        {
            tweens.Remove(tween);
        }

        /// <summary>
        /// Stops all tweens from running.
        /// </summary>
        public void StopAll()
        {
            tweens.Clear();
        }

        /// <summary>
        /// Determines if a tween is playing, based on its name.
        /// <para>If the name is not unique, this may return a false positive. See IsPlaying(Tween).</para>
        /// </summary>
        /// <param name="tweenName">The name of the tween</param>
        /// <returns>True if the tween is playing now or immediately</returns>
        public bool IsPlaying(string tweenName)
        {
            return tweens.Exists(tween => tween.name == tweenName) || tweensToAdd.Exists(tween => tween.name == tweenName);
        }

        /// <summary>
        /// Determines if a tween instance is playing.
        /// </summary>
        /// <param name="tweenName">The tween instance</param>
        /// <returns>True if the tween is playing now or immediately</returns>
        public bool IsPlaying(Tween tween)
        {
            return tweens.Contains(tween) || tweensToAdd.Contains(tween);
        }

        /// <summary>
        /// Determines if a tween is playing, based on its name, and returns it.
        /// </summary>
        /// <param name="tweenName">The name of the tween</param>
        /// <returns>A tween which is playing now or immediately, or null if no such tween is found</returns>
        public Tween GetPlaying(string tweenName)
        {
            Tween tween = tweens.Find(t => t.name == tweenName);
            if (tween == null) tweensToAdd.Find(t => t.name == tweenName);
            return tween;
        }

        /// <summary>
        ///  Plays a chain of template tweens, one after another.
        /// <para>Makes a clone of each tween first</para>
        /// </summary>
        /// <param name="tweenNames">An array of template tween names</param>
        public void PlayChain(params string[] tweenNames)
        {
            Tween[] tweensToChain = new Tween[tweenNames.Length];
            for (int i = 0; i < tweenNames.Length; i++)
                tweensToChain[i] = GetTemplate(tweenNames[i]).Clone();
            PlayChain(tweensToChain);
        }

        /// <summary>
        /// Plays a chain of tween instances, one after another.
        /// <para>Will modify each tween to invoke Play on the next tween when end one ends</para>
        /// </summary>
        /// <param name="tweensToChain">An array of tweens</param>
        public void PlayChain(params Tween[] tweensToChain)
        {
            if (tweensToChain.Length == 0)
                return;

            for (int i = 0; i < tweensToChain.Length - 1; i++)
            {
                Tween thisTween = tweensToChain[i];
                Tween nextTween = tweensToChain[i + 1];
                UnityAction<Tween> action = new UnityAction<Tween>(delegate { Play(nextTween); });
                action += delegate { thisTween.options.events.End.RemoveListener(action); };
                thisTween.options.events.End.AddListener(action);
            }
            Play(tweensToChain[0]);
        }

        /// <summary>
        /// Returns the template Tween with supplied name.
        /// <para>If you modify this directly, you are modifying the template.
        // You probably want to make a copy with Tween.Clone(), modify the copy, and pass that to Play().</para>
        /// </summary>
        /// <param name="tweenName">The unique name of the template Tween</param>
        /// <returns>The template Tween instance, or throws an exception if not found</returns>
        public Tween GetTemplate(string tweenName)
        {
            Tween tween = tweenTemplates.Find(x => x.name == tweenName);
            if (tween == null)
                throw new KeyNotFoundException("Tween not found:" + tweenName);
            return tween;
        }

        internal Tween GetTemplateByIndex(int index)
        {
            if (index >= tweenTemplates.Count)
                return null;
            return tweenTemplates[index];
        }

        /// <summary>
        /// Determines if the named template Tween exists.
        /// </summary>
        /// <param name="tweenName">The name of the template Tween</param>
        /// <returns>True if there is a template Tween defined with the supplied name</returns>
        public bool TemplateExists(string tweenName)
        {
            Tween tween = tweenTemplates.Find(x => x.name == tweenName);
            return tween != null;
        }

        /// <summary>
        /// Adds the tween instance to the list of template Tweens.
        /// <para>Throws an exception if the tween name is not unique among all the templates</para>
        /// </summary>
        /// <param name="tween">The tween instance</param>
        public void AddTemplate(Tween tween)
        {
            if (TemplateExists(tween.name))
                throw new UnityException("Cannot add tween " + tween.name + " as a template; name already in use");

            tweenTemplates.Add(tween);
        }

        void Awake()
        {
            if (TweenManager.instance == null || this.primaryInstance)
                TweenManager.instance = this;
        }

        /// <summary>
        /// Updates all running tweens. Removes finished tweens.
        /// </summary>
        void Update()
        {
            ProcessActiveTweens();
            ProcessNewTweens();
        }

        public void ProcessActiveTweens()
        {
            // Process all active tweens
            foreach(Tween tween in tweens)
            {
                // Tween loop has run its delay
                if (tween.delayRemaining > 0)
                {
                    tween.delayRemaining -= Time.deltaTime;
                    if (tween.delayRemaining > 0)
                        continue;
                    tween.delayRemaining = 0f;
                    tween.options.events.Loop.Invoke(tween);
                }

                // Tween loop has run its duration
                else
                {
                    tween.timeRemaining -= Time.deltaTime;
                    if (tween.timeRemaining < 0f)
                    {
                        tween.timeRemaining = 0f;
                        tween.loopsRemaining--;
                    }
                }

                ApplyTween(tween);

                if (tween.timeRemaining <= 0f)
                {
                    // Tween has finished loop
                    if (tween.loopsRemaining != 0)
                        tween.Activate(true);

                    // Tween has finished all iterations
                    else tween.options.events.End.Invoke(tween);
                }
            }

            // Remove any inactive tweens from list
            tweens.RemoveAll(t => t.loopsRemaining == 0);
        }

        public void ProcessNewTweens()
        {
            // Add new tweens to active tweens
            while (tweensToAdd.Count > 0)
            {
                // Pop tween from list
                Tween tween = tweensToAdd[0];
                tweensToAdd.RemoveAt(0);

                // Start it up
                tween.Activate();
                tweens.Add(tween);
                if (tween.options.events != null) tween.options.events.Start.Invoke(tween);
                ApplyTween(tween);
            }
        }

        private void ApplyTween(Tween tween)
        {
            float timeRatio = tween.timeRemaining / tween.duration;

            foreach(TweenProperty tp in new TweenProperty[] { tween.position, tween.rotation, tween.scale, tween.color, tween.value })
            {
                if (!tp.enabled) continue;

                // Update tween time and options
                float t = timeRatio;
                if (tp.options.randomize)
                    t = Random.Range(0f, 1.0f);
                if (tp.options.pingPong)
                    t = (t < 0.5f ? 1f - t * 2 : (t - 0.5f) * 2);
                if (!tp.options.reverse) t = 1 - t;
                if (tp.options.easing.length > 0)
                    t = tp.options.easing.Evaluate(t);

                // Now tween
                tp.value = TweenValue.Vector4(Vector4.LerpUnclamped(tp.startValue.value, tp.endValue.value, t));

                // WaitThenDest only updates on the last frame
                if (tp.range == TweenPropertyRange.Dest && tween.timeRemaining > 0f && tween.loopsRemaining != 0)
                    continue;

                // Apply tween to object
                tp.Apply(tween);
            }

            // Call change event
            if (tween.options.events != null)
                tween.options.events.Change.Invoke(tween);
        }

        void OnValidate()
        {
            // Provide default values
            tweenTemplates.ForEach(tween =>
            {
                if (tween.options.loops == 0)
                    tween.options.loops = 1;

                if (tween.initialized)
                    return;

                tween.name = "default";
                tween.duration = 1;
                tween.initialized = true;
            });
        }

        public void DebugCallback(Tween t)
        {
            string resp = "Tween name:" + t.name +
                " time:" + t.timeRemaining + "/" + t.duration +
                " loops:" + t.loopsRemaining + "/" + t.options.loops + " ";
            if (t.position.enabled) resp += "(Position:" + t.position.value.Vector3() + ") ";
            if (t.rotation.enabled) resp += "(Rotation:" + t.rotation.value.Vector3() + ") ";
            if (t.scale.enabled) resp += "(Scale:" + t.scale.value.Vector3() + ") ";
            if (t.color.enabled) resp += "(Color:" + t.color.value.Color() + ") ";
            if (t.value.enabled) resp += "(Value:" + t.value.value.Vector4() + ") ";
            Debug.Log(resp);
        }
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [System.Serializable]
    public class Tween
    {
        [Tooltip("The unique name of the tween; required for template tweens")]
        public string name;

        [Tooltip("The duration of the tween in seconds, must be > 0")]
        public float duration;

        [Tooltip("A GameObject; if you are doing color/alpha tweening, this must contain a SpriteRenderer, Rendererer or Text")]
        public GameObject target;

        [Tooltip("Advanced Options")]
        public TweenOptions options;

        public TweenPropertyPosition position;
        public TweenPropertyRotation rotation;
        public TweenPropertyScale scale;
        public TweenPropertyColor color;
        public TweenPropertyValue value;

        //////////////////////////////////////////////////////

        [HideInInspector]
        public int loopsRemaining; // the number of loops still to run

        [HideInInspector]
        public float delayRemaining; // The amount of time left to delay the end start of the subsequent loop        

        [HideInInspector]
        public float timeRemaining; // the time remaining to tween

        [HideInInspector]
        public bool initialized;

        [System.NonSerialized]
        public SpriteRenderer spriteRenderer;

        [System.NonSerialized]
        public Renderer renderer;

        [System.NonSerialized]
        public Text text;

        public Tween()
        {
            this.Init("default", 1, null);
        }

        /// <summary>
        ///  Constructs a new Tween from an existing Tween instance. Also see Clone().
        /// <para>Useful for copying a template tween</para>
        /// /// </summary>
        /// <param name="tween">The tween instance to copy</param>
        /// <param name="includeEvents">If true, shares the events from the tween being copied; if false, starts with no events</param>
        public Tween(Tween tween, bool includeEvents = false)
        {
            TweenOptions options = tween.options.Clone(includeEvents);
            Init(name, duration, target, position, rotation, scale, color, value, options);
        }

        /// <summary>
        /// Constructor. See Tween class for parameter definitions. I'm lazy.
        /// </summary>
        public Tween(string name, float duration, GameObject target, TweenPropertyPosition position, TweenPropertyRotation rotation,
            TweenPropertyScale scale, TweenPropertyColor color, TweenPropertyValue value, TweenOptions options = null)
        {
            Init(name, duration, target, position, rotation, scale, color, value, options);
        }

        /// <summary>
        /// Clones the tween and replaces the target the with components of the GameObject you supply
        /// </summary>
        /// <param name="go">The game object target to replace</param>
        /// <param name="includeEvents">If true, the clone shares the TweenEvents object with the Tween being cloned</param>
        /// <returns>A copy of the Tween with the target(s) replaced</returns>
        public Tween Clone(GameObject go = null, bool includeEvents = false)
        {
            return new Tween(this, includeEvents);
        }

        /// <summary>
        /// Called by Play() to prepare the Tween for updating.
        /// </summary>
        public void Activate(bool reactivatingFromLoop = false)
        {
            if (duration <= 0)
                throw new UnityException("Duration must be > 0");

            // Update caches for non-transform components
            this.spriteRenderer = target == null ? null : target.GetComponent<SpriteRenderer>();
            this.renderer = target == null ? null : target.GetComponent<Renderer>();
            this.text = target == null ? null : target.GetComponent<Text>();

            // Activate each property
            position.Activate(this);
            rotation.Activate(this);
            scale.Activate(this);
            color.Activate(this);
            value.Activate(this);

            // Setup timers
            timeRemaining = duration;
            if (reactivatingFromLoop)
                delayRemaining = options.loopDelay;
            else
            {
                delayRemaining = 0f;
                loopsRemaining = options.loops;
            }
        }

        private void Init(string name, float duration, GameObject target, TweenPropertyPosition position = null, TweenPropertyRotation rotation = null,
            TweenPropertyScale scale = null, TweenPropertyColor color = null, TweenPropertyValue value = null, TweenOptions options = null)
        {
            this.name = name.IsEmpty() ? "default" : name;
            this.duration = duration;
            this.target = target;
            this.options = options == null ? new TweenOptions() : options;
            this.position = position == null ? new TweenPropertyPosition() : position;
            this.rotation = rotation == null ? new TweenPropertyRotation() : rotation;
            this.scale = scale == null ? new TweenPropertyScale() : scale;
            this.color = color == null ? new TweenPropertyColor() : color;
            this.value = value == null ? new TweenPropertyValue() : value;
        }
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [System.Serializable]
    abstract public class TweenProperty
    {
        [HideInInspector]
        public bool enabled;

        [Tooltip("Determines if you are supplying both source and dest, or if you're fetching either value from the object being tweened")]
        public TweenPropertyRange range;

        [Tooltip("The starting value for the tween")]
        public TweenValue source;

        [Tooltip("the ending value for the tween")]
        public TweenValue dest;

        [Tooltip("Click any axis to freeze it; this prevents the axis from being modified by the tween; for example, to tween color alpha you'll want to freeze axes XYZ")]
        public TweenFrozenAxes frozenAxes;

        [Tooltip("Advanced Options")]
        public TweenPropertyOptions options = new TweenPropertyOptions();

        [HideInInspector]
        public TweenValue startValue; // private, used during tweening

        [HideInInspector]
        public TweenValue endValue; // private, used during tweening

        [HideInInspector]
        public TweenValue value; // the current value of the tween

        public void Activate(Tween tween)
        {
            if (!enabled)
                return;

            TweenValue? rawTargetValue = GetTargetValue(tween);
            TweenValue targetValue = (rawTargetValue == null ? new TweenValue() : (TweenValue) rawTargetValue);

            switch (range)
            {
                case TweenPropertyRange.Dest:
                case TweenPropertyRange.TargetToDest:
                    startValue = targetValue;
                    endValue = options.relativeVals ? targetValue + dest : dest;
                    break;

                case TweenPropertyRange.SourceToDest:
                    startValue = options.relativeVals ? targetValue + source : source;
                    endValue = options.relativeVals ? startValue + dest : dest;
                    break;

                case TweenPropertyRange.SourceToTarget:
                    startValue = options.relativeVals ? targetValue + source : source;
                    endValue = targetValue;
                    break;
            }

            // TODO This sucks
            if (frozenAxes.x) startValue.value.x = endValue.value.x = targetValue.value.x;
            if (frozenAxes.y) startValue.value.y = endValue.value.y = targetValue.value.y;
            if (frozenAxes.z) startValue.value.z = endValue.value.z = targetValue.value.z;
            if (frozenAxes.w) startValue.value.w = endValue.value.w = targetValue.value.w;

            value = startValue;
        }
        abstract public TweenValue? GetTargetValue(Tween tween);
        abstract public void Apply(Tween tween);
        abstract public void AddValueFields(SerializedProperty value, Rect pos, float adjustedWidth, bool isTweenValue = false);
    }

    [System.Serializable]
    public class TweenPropertyPosition : TweenProperty
    {
        public override TweenValue? GetTargetValue(Tween tween)
        {
            tween.target.ThrowIfNull("Target not assigned");
            if (options.local) return TweenValue.Vector3(tween.target.transform.localPosition);
            return TweenValue.Vector3(tween.target.transform.position);
        }

        public override void Apply(Tween tween)
        {
            if (options.local) tween.target.transform.localPosition = value.Vector3();
            else tween.target.transform.position = value.Vector3();
        }

        public override void AddValueFields(SerializedProperty value, Rect pos, float adjustedWidth, bool isTweenValue = false)
        {
            adjustedWidth *= 0.333f;
            Helper.AddField(value, pos, "x", adjustedWidth, isTweenValue ? frozenAxes.x : false, 10f);
            Helper.AddField(value, pos, "y", adjustedWidth, isTweenValue ? frozenAxes.y : false, 10f);
            Helper.AddField(value, pos, "z", adjustedWidth, isTweenValue ? frozenAxes.z : false, 10f);
        }
    }

    [System.Serializable]
    public class TweenPropertyRotation : TweenProperty
    {
        public override TweenValue? GetTargetValue(Tween tween)
        {
            tween.target.ThrowIfNull("Target not assigned");
            if (options.local) return TweenValue.Vector3(tween.target.transform.localEulerAngles);
            return TweenValue.Vector3(tween.target.transform.eulerAngles);
        }

        public override void Apply(Tween tween)
        {
            if (options.local) tween.target.transform.localEulerAngles = value.Vector3();
            else tween.target.transform.eulerAngles = value.Vector3();
        }

        public override void AddValueFields(SerializedProperty value, Rect pos, float adjustedWidth, bool isTweenValue = false)
        {
            adjustedWidth *= 0.333f;
            Helper.AddField(value, pos, "x", adjustedWidth, isTweenValue ? frozenAxes.x : false, 10f);
            Helper.AddField(value, pos, "y", adjustedWidth, isTweenValue ? frozenAxes.y : false, 10f);
            Helper.AddField(value, pos, "z", adjustedWidth, isTweenValue ? frozenAxes.z : false, 10f);
        }
    }

    [System.Serializable]
    public class TweenPropertyScale : TweenProperty
    {
        public override TweenValue? GetTargetValue(Tween tween)
        {
            tween.target.ThrowIfNull("Target not assigned");
            return TweenValue.Vector3(tween.target.transform.localScale);
        }
        public override void Apply(Tween tween)
        {
            tween.target.transform.localScale = value.Vector3();
        }

        public override void AddValueFields(SerializedProperty value, Rect pos, float adjustedWidth, bool isTweenValue = false)
        {
            adjustedWidth *= 0.333f;
            Helper.AddField(value, pos, "x", adjustedWidth, isTweenValue ? frozenAxes.x : false, 10f);
            Helper.AddField(value, pos, "y", adjustedWidth, isTweenValue ? frozenAxes.y : false, 10f);
            Helper.AddField(value, pos, "z", adjustedWidth, isTweenValue ? frozenAxes.z : false, 10f);
        }
    }

    [System.Serializable]
    public class TweenPropertyColor : TweenProperty
    {
        public override TweenValue? GetTargetValue(Tween tween)
        {
            tween.target.ThrowIfNull("Target not assigned");
            if (tween.spriteRenderer != null)
                return TweenValue.Color(tween.spriteRenderer.color);
            else if (tween.text != null)
                return TweenValue.Color(tween.text.color);
            else if (tween.renderer != null)
                return TweenValue.Color(tween.renderer.material.color);
            else throw new UnityException("TweenType.Color requires target to have a SpriteRenderer, Renderer or Text component");
        }

        public override void Apply(Tween tween)
        {
            if (tween.spriteRenderer != null) tween.spriteRenderer.color = value.Color();
            else if (tween.text != null) tween.text.color = value.Color();
            else if (tween.renderer != null) tween.renderer.material.color = value.Color();
            else Debug.Log("TweenType.Color requires target to have a SpriteRenderer, Renderer or Text component");
        }

        public override void AddValueFields(SerializedProperty prop, Rect rect, float width, bool isTweenValue = false)
        {
            width /= 5;
            Helper.AddField(prop, rect, "x", width, isTweenValue ? frozenAxes.x : false, 10f, "r");
            Helper.AddField(prop, rect, "y", width, isTweenValue ? frozenAxes.y : false, 10f, "g");
            Helper.AddField(prop, rect, "z", width, isTweenValue ? frozenAxes.z : false, 10f, "b");
            Helper.AddField(prop, rect, "w", width, isTweenValue ? frozenAxes.w : false, 10f, "a");

            if (isTweenValue)
            {
                Rect colorRect = new Rect(rect.x + Helper.lastX, rect.y, width, rect.height);
                prop.vector4Value = (Vector4) EditorGUI.ColorField(colorRect, GUIContent.none,
                    (Color) prop.vector4Value, false, !frozenAxes.w, false, null);
            }
        }
    }

    [System.Serializable]
    public class TweenPropertyValue : TweenProperty
    {
        public TweenPropertyValueType type;

        public override TweenValue? GetTargetValue(Tween tween)
        {
            return null;
        }

        public override void Apply(Tween tween) { }

        public override void AddValueFields(SerializedProperty prop, Rect rect, float width, bool isTweenValue = false)
        {
            int valueIdx = (int) type;
            width *= 1f / (valueIdx + 1);
            AddValueSingleField(prop, rect, width, isTweenValue ? frozenAxes.x : false, "x", new string[] { "value", "x", "x", "x", "r" }, valueIdx);
            AddValueSingleField(prop, rect, width, isTweenValue ? frozenAxes.y : false, "y", new string[] { "", "y", "y", "y", "g" }, valueIdx);
            AddValueSingleField(prop, rect, width, isTweenValue ? frozenAxes.z : false, "z", new string[] { "", "", "z", "z", "b" }, valueIdx);
            AddValueSingleField(prop, rect, width, isTweenValue ? frozenAxes.w : false, "w", new string[] { "", "", "", "w", "a" }, valueIdx);

            if (isTweenValue && type == TweenPropertyValueType.Color)
            {
                Rect colorRect = new Rect(rect.x + Helper.lastX, rect.y, width, rect.height);
                prop.vector4Value = (Vector4) EditorGUI.ColorField(colorRect, GUIContent.none,
                    (Color) prop.vector4Value, false, !frozenAxes.w, false, null);
            }
        }

        private void AddValueSingleField(SerializedProperty prop, Rect rect, float width, bool show, string field, string[] labels, int labelIdx)
        {
            string label = labels[labelIdx];
            if (!label.IsEmpty())
                Helper.AddField(prop, rect, field, width, show, 10f * label.Length, label);
        }
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [System.Serializable]
    public struct TweenFrozenAxes
    {
        public bool x;
        public bool y;
        public bool z;
        public bool w;
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [System.Serializable]
    public class TweenEvents
    {
        [Tooltip("Invoked when the tween is activated, but before the object gets its first update")]
        public TweenEvent Start = new TweenEvent();

        [Tooltip("Invoked after the tween is updated, use this for Float tweens")]
        public TweenEvent Change = new TweenEvent();

        [Tooltip("Invoked when an updating tween is has finished but before it is removed; you can set timeRemaining to duration to prevent the tween's removal")]
        public TweenEvent End = new TweenEvent();

        [Tooltip("Invoked when an looping tween has looped; if there is a loop delay, the event occurs after the delay")]
        public TweenEvent Loop = new TweenEvent();
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [System.Serializable]
    public class TweenOptions
    {
        [Tooltip("The number of times to play the tween; use -1 for infinite looping")]
        public int loops;

        [Tooltip("An optional looping delay (in seconds) that occurs between loops; ignored if loops is 1")]
        public float loopDelay;

        [Tooltip("Optional event notifications")]
        public TweenEvents events = new TweenEvents();

        public TweenOptions() { }

        public TweenOptions(TweenOptions options, bool includeEvents = false)
        {
            loops = options.loops;
            loopDelay = options.loopDelay;

            if (includeEvents)
                events = options.events;
        }

        public TweenOptions Clone(bool includeEvents = false)
        {
            return new TweenOptions(this, includeEvents);
        }
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [System.Serializable]
    public class TweenPropertyOptions
    {
        [Tooltip("For position and rotation, if true, enables local space positioning instead of world space")]
        public bool local;

        [Tooltip("If true, source is relative to the target, and dest is relative to the source")]
        public bool relativeVals;

        [Tooltip("If true, plays forward and back again; the total duration does not change")]
        public bool pingPong;

        [Tooltip("If true, plays tween in reverse (1-0 instead of 0-1)")]
        public bool reverse;

        [Tooltip("If true, randomizes the time value; easing, reverse, etc still apply")]
        public bool randomize;

        [Tooltip("An optional easing curve")]
        public AnimationCurve easing;

        public TweenPropertyOptions() { }

        public TweenPropertyOptions(TweenPropertyOptions options)
        {
            relativeVals = options.relativeVals;
            reverse = options.reverse;
            easing = options.easing;
            pingPong = options.pingPong;
        }

        public TweenPropertyOptions Clone()
        {
            return new TweenPropertyOptions();
        }
    }

    //////////////////////////////////////////////////////////////////////////////// 

    /// <summary>
    /// A wrapper for a tweenable value. 
    /// <para>The underlying type is a Vector4. Use the GetXXX() functions to retrieve the native value 
    /// relevant to the TweenType.</para>
    /// </summary>
    [System.Serializable]
    public struct TweenValue
    {
        public Vector4 value;

        public TweenValue(float x = 0f, float y = 0f, float z = 0f, float w = 0f)
        {
            value = new Vector4(x, y, z, w);
        }

        public TweenValue(Vector4 vec)
        {
            value = vec;
        }

        public float X() { return value.x; }
        public float Red() { return value.x; }
        public float Y() { return value.y; }
        public float Green() { return value.y; }
        public float Z() { return value.z; }
        public float Blue() { return value.z; }
        public float Angle() { return value.z; }
        public float W() { return value.w; }
        public float Alpha() { return value.w; }
        public float Float() { return value.x; }

        /// <returns>2D position and scale</returns>
        public Vector2 Vector2() { return (Vector2) value; }

        /// <returns>All 3D tween types</returns>
        public Vector3 Vector3() { return (Vector3) value; }

        /// <returns>The native Vector4</returns>
        public Vector4 Vector4() { return value; }

        /// <returns>Colors</returns>
        public Color Color() { return (Color) value; }

        public static TweenValue Float(float f) { return new TweenValue(f, 0, 0, 0); }
        public static TweenValue Vector2(Vector2 v) { return new TweenValue((Vector4) v); }
        public static TweenValue Vector3(Vector3 v) { return new TweenValue((Vector4) v); }
        public static TweenValue Vector4(Vector4 v) { return new TweenValue(v); }
        public static TweenValue Color(Color c) { return new TweenValue((Vector4) c); }

        public static TweenValue operator + (TweenValue tv1, TweenValue tv2)
        {
            return new TweenValue(tv1.value + tv2.value);
        }
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [System.Serializable]
    public class TweenEvent : UnityEvent<Tween> { }

    ////////////////////////////////////////////////////////////////////////////////     

    [System.Serializable]
    public enum TweenPropertyValueType
    {
        Float, // All value tweens do not modify anything - but they invoke Change events
        Vector2,
        Vector3,
        Vector4,
        Color
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [System.Serializable]
    public enum TweenPropertyRange
    {
        SourceToDest, // tweens from the source value to the destination value
        TargetToDest, // tweens from the target's current value to the destination value
        SourceToTarget, // tweens from the source value to the target's current value
        Dest // does not tween, merely sets the target's value after duration + delay
    }

    //////////////////////////////////////////////////////////////////////////////// 
    ///////////////////////////// E D I T O R //////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////// 

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof (Tween))]
    public class TweenPD : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.PropertyField(pos, prop, label, true);
            if (prop.isExpanded)
            {
                float width = 80f;
                GUI.enabled = Application.isPlaying;
                if (GUI.Button(new Rect(pos.xMin + (pos.width - width) / 2, pos.yMax - 20f, width, 20f),
                        new GUIContent("Live Preview", "When application is running, press this to preview the tween")))
                {
                    TweenManager tm = (TweenManager) prop.serializedObject.targetObject;
                    string name = prop.serializedObject.FindProperty(prop.propertyPath + ".name").stringValue;
                    tm.Stop(name);
                    tm.Play(name);
                }
            }
            GUI.enabled = true;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            if (prop.isExpanded)
                return EditorGUI.GetPropertyHeight(prop) + 25f;
            return EditorGUI.GetPropertyHeight(prop);
        }
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [CustomPropertyDrawer(typeof (TweenPropertyPosition))]
    [CustomPropertyDrawer(typeof (TweenPropertyRotation))]
    [CustomPropertyDrawer(typeof (TweenPropertyScale))]
    [CustomPropertyDrawer(typeof (TweenPropertyColor))]
    [CustomPropertyDrawer(typeof (TweenPropertyValue))]
    public class TweenPropertyPD : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            TweenProperty tp = Helper.GetTweenProperty(prop);
            if (tp == null)
                return;

            if (!tp.enabled)
                EditorGUI.LabelField(pos, prop.displayName);

            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float x = (pos.width < 338f ? 120f : (pos.width - 338f) * 0.45f + 120f);
            Rect r = new Rect(pos.x + x, pos.y, pos.width - x, EditorGUIUtility.singleLineHeight);
            bool wasEnabled = tp.enabled;
            tp.enabled = EditorGUI.ToggleLeft(r, "Enabled", tp.enabled);
            EditorGUI.indentLevel = indentLevel;

            if (tp.enabled && !wasEnabled)
                prop.isExpanded = true;

            if (tp.enabled)
                EditorGUI.PropertyField(pos, prop, new GUIContent(prop.displayName), true);
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            TweenProperty tp = Helper.GetTweenProperty(prop);
            if (tp == null)
                return -EditorGUIUtility.singleLineHeight;
            else if (!tp.enabled || !prop.isExpanded)
                return EditorGUIUtility.singleLineHeight;
            return EditorGUI.GetPropertyHeight(prop);
        }
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [CustomPropertyDrawer(typeof (TweenValue))]
    public class TweenValuePD : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if (IsHidden(prop)) return;

            TweenProperty tp = Helper.GetTweenProperty(prop);
            SerializedProperty value = prop.FindPropertyRelative("value");

            EditorGUI.BeginProperty(pos, new GUIContent("Target"), prop);
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel - 2;
            Helper.lastX = 30f;
            Helper.AddLabel(pos, prop.displayName, 90f);
            EditorGUI.indentLevel = 0;
            Helper.lastX = (pos.width < 338f ? 120f : (pos.width - 338f) * 0.45f + 120f);
            float adjustedWidth = pos.width - Helper.lastX;
            tp.AddValueFields(value, pos, adjustedWidth, true);
            EditorGUI.indentLevel = indentLevel;
            EditorGUI.EndProperty();
        }
        private bool IsHidden(SerializedProperty prop)
        {
            TweenProperty tp = Helper.GetTweenProperty(prop);
            if (tp == null)
                return true;

            // TODO Use TP above
            string tweenPath = prop.propertyPath.Substring(0, prop.propertyPath.LastIndexOf("."));
            string rangeTypePath = tweenPath + ".range";
            SerializedProperty rangeTypeProp = prop.serializedObject.FindProperty(rangeTypePath);

            TweenPropertyRange rangeType = (TweenPropertyRange) rangeTypeProp.enumValueIndex;
            return (rangeType == TweenPropertyRange.SourceToTarget && prop.name == "dest") ||
                (rangeType == TweenPropertyRange.TargetToDest && prop.name == "source") ||
                (rangeType == TweenPropertyRange.Dest && prop.name == "source");
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            if (IsHidden(prop)) return -EditorGUIUtility.standardVerticalSpacing;
            return EditorGUI.GetPropertyHeight(prop, label, false);
        }
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [CustomPropertyDrawer(typeof (TweenFrozenAxes))]
    public class TweenFrozenAxesPD : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            TweenProperty tp = Helper.GetTweenProperty(prop);
            if (tp == null || (tp is TweenPropertyValue && ((TweenPropertyValue) tp).type == TweenPropertyValueType.Float))
                return;

            EditorGUI.BeginProperty(pos, label, prop);
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel - 2;
            Helper.lastX = 30f;
            Helper.AddLabel(pos, pos.width > 360 ? "Frozen Axes" : "Frozen", 90f);
            EditorGUI.indentLevel = 0;
            Helper.lastX = (pos.width < 338f ? 120f : (pos.width - 338f) * 0.45f + 120f);
            float adjustedWidth = pos.width - Helper.lastX;
            tp.AddValueFields(prop, pos, adjustedWidth);
            EditorGUI.indentLevel = indentLevel;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            TweenProperty tp = Helper.GetTweenProperty(prop);
            if (tp == null)
                return -EditorGUIUtility.standardVerticalSpacing;
            if (tp is TweenPropertyValue && ((TweenPropertyValue) tp).type == TweenPropertyValueType.Float)
            {
                tp.frozenAxes = new TweenFrozenAxes(); // reset to zero, to ensure no hidden freezes
                return -EditorGUIUtility.standardVerticalSpacing;
            }
            return EditorGUI.GetPropertyHeight(prop, label, false);
        }
    }

    //////////////////////////////////////////////////////////////////////////////// 

    internal class Helper
    {
        public static float lastX = 0;

        public static Tween GetTween(SerializedProperty prop)
        {
            TweenManager tm = (TweenManager) prop.serializedObject.targetObject;
            Match match = Regex.Match(prop.propertyPath, @"^tweenTemplates\.Array\.data\[(\d+)\]\.(\w+)\.");
            return tm.GetTemplateByIndex(int.Parse(match.Groups[1].Value));
        }

        public static TweenProperty GetTweenProperty(SerializedProperty prop)
        {
            TweenManager tm = (TweenManager) prop.serializedObject.targetObject;
            Match match = Regex.Match(prop.propertyPath, @"^tweenTemplates\.Array\.data\[(\d+)\]\.(\w+)");
            Tween tween = tm.GetTemplateByIndex(int.Parse(match.Groups[1].Value));
            if (tween == null)
                return null;
            switch (match.Groups[2].Value)
            {
                case "position":
                    return tween.position;
                case "scale":
                    return tween.scale;
                case "rotation":
                    return tween.rotation;
                case "color":
                    return tween.color;
                case "value":
                    return tween.value;
            }
            throw new UnityException("Unknown type:" + match.Groups[2].Value);
        }

        public static void AddLabel(Rect pos, string propName, float width)
        {
            Rect fieldRect = new Rect(pos.x + Helper.lastX, pos.y, width, pos.height);
            EditorGUI.PrefixLabel(fieldRect, new GUIContent(propName));
            Helper.lastX += width;
        }

        public static void AddField(SerializedProperty prop, Rect pos, string propName, float width, bool isDisabled = false,
            float labelWidth = 10f, string altLabel = null)
        {
            GUI.enabled = !isDisabled;
            string label = (altLabel == null ? propName : altLabel);
            SerializedProperty fieldProp = prop.FindPropertyRelative(propName);
            Rect fieldRect = new Rect(pos.x + Helper.lastX, pos.y, width, pos.height);
            EditorGUI.LabelField(fieldRect, label);
            fieldRect.x += labelWidth;
            fieldRect.width = width - labelWidth - 5;
            EditorGUI.PropertyField(fieldRect, fieldProp, GUIContent.none);
            Helper.lastX += width;
            GUI.enabled = true;
        }
    }
#endif
}