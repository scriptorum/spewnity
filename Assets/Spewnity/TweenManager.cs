using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

// TODO Cache TweenTemplate names in dictionary?
// TODO Add Reverse, PingPong, although some of this can be approximated with easing
// TODO A Vec4 lerp is only needed for color, other TweenTypes will be more performant if you lerp just the parts you need.
// TODO Add ColorWithoutAlpha? It's already a pretty long list!
// TODO Does it make sense to create a TweenTarget class as a Transform, and pull the Text or SpriteRenderer from it? What if a game object has both?
// TODO Editor and PropertyDrawer work is such a drag - look into attributes and helper functions
// TODO Should Lerp be clamped for things like Colors?
// TODO Create a more formal structure to do chaining and layering of tweens. Maybe you pick a target, and then have a set of TweenRules per target, each with their own delay?
// TODO Is it possible to clone a UnityEvent? It might not be necessary but I'd like to be able to clone the TweenEvents object properly.
namespace Spewnity
{
    /// <summary>
    /// Another tweening system.
    /// <para>You can specify persistant tweens in the inspector or ad-hoc ones through the API. Persistent tweens can be run 
    /// as defined with Play(string), or they can be used as templates: Clone() and modify the tween, and then Play(tween).</para>
    /// <para>The tweens system supports 2D and 3D transform tweening, as well as SpriteRenderer and Text color and 
    /// alpha. Freely tween independent floats, vectors, and colors using the event system. Supports easing, ping-pong, 
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
                tween.timeRemaining -= Time.deltaTime;
                if (tween.timeRemaining < 0f)
                {
                    tween.timeRemaining = 0f;
                    tween.loopsRemaining--;
                }

                ApplyTween(tween);

                if (tween.timeRemaining <= 0f)
                {
                    // Tween has finished loop
                    if (tween.loopsRemaining != 0)
                        tween.Activate(false);

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
                Tween tween = tweensToAdd[0];
                tweensToAdd.RemoveAt(0);

                tween.Activate();
                tweens.Add(tween);
                if (tween.options.events != null) tween.options.events.Start.Invoke(tween);

                ApplyTween(tween);
            }
        }

        private void ApplyTween(Tween tween)
        {
            Color color;
            Vector3 vec;
            Vector4 value = new Vector4();

            float t = tween.timeRemaining / tween.duration;
            if (tween.options.pingPong)
                t = (t < 0.5f ? 1f - t * 2 : (t - 0.5f) * 2);
            if (!tween.options.reverse) t = 1 - t;
            if (tween.options.easing.length > 0)
                t = tween.options.easing.Evaluate(t);
            value = Vector4.LerpUnclamped(tween.startValue.value, tween.endValue.value, t);
            tween.value.value = value;

            // JustWait never updates
            if (tween.rangeType == RangeType.JustWait)
                return;

            // WaitThenDest only updates on the last frame
            if (tween.rangeType == RangeType.WaitThenDest && tween.timeRemaining > 0f && tween.loopsRemaining != 0)
                return;

            // Apply tween to object
            switch (tween.tweenType)
            {
                case TweenType.Float:
                case TweenType.Vector2:
                case TweenType.Vector3:
                case TweenType.Vector4:
                case TweenType.Color:
                    break; // nothing to do here, no object, just callback

                case TweenType.SpriteRendererAlpha:
                    color = tween.spriteRenderer.color;
                    color.a = value.x;
                    tween.spriteRenderer.color = color;
                    break;

                case TweenType.TextAlpha:
                    color = tween.text.color;
                    color.a = value.x;
                    tween.text.color = color;
                    break;

                case TweenType.SpriteRendererColor:
                    tween.spriteRenderer.color = (Color) value;
                    break;

                case TweenType.TextColor:
                    tween.text.color = (Color) value;
                    break;

                case TweenType.LocalPosition2D:
                    tween.transform.localPosition = new Vector3(value.x, value.y, tween.transform.localPosition.z);
                    break;

                case TweenType.LocalPosition3D:
                    tween.transform.localPosition = (Vector3) value;
                    break;

                case TweenType.Position2D:
                    tween.transform.position = new Vector3(value.x, value.y, tween.transform.position.z);
                    break;

                case TweenType.Position3D:
                    tween.transform.position = (Vector3) value;
                    break;

                case TweenType.Rotation2D:
                    vec = tween.transform.eulerAngles;
                    vec.z = value.x;
                    tween.transform.eulerAngles = vec;
                    break;

                case TweenType.LocalRotation2D:
                    vec = tween.transform.localEulerAngles;
                    vec.z = value.x;
                    tween.transform.localEulerAngles = vec;
                    break;

                case TweenType.Rotation3D:
                    tween.transform.eulerAngles = (Vector3) value;
                    break;

                case TweenType.Scale2D:
                    tween.transform.localScale = new Vector3(value.x, value.y, tween.transform.localScale.z);
                    break;

                case TweenType.Scale3D:
                    tween.transform.localScale = (Vector3) value;
                    break;

                case TweenType.None:
                    break;

                default:
                    Debug.Log("Unknown TweenType:" + tween.tweenType);
                    break;
            }

            if (tween.options.events != null)
                tween.options.events.Change.Invoke(tween);
        }

        void OnValidate()
        {
            // Provide default values
            tweenTemplates.ForEach(tween => { if (tween.options.loops == 0) tween.options.loops = 1; });
        }

        public void DebugCallback(Tween t)
        {
            Debug.Log("Tween name:" + t.name +
                " time:" + t.timeRemaining + "/" + t.duration +
                " loops:" + t.loopsRemaining + "/" + t.options.loops +
                " value:" + t.value.Vector4());
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
        [Tooltip("A target, any GameObject since they all have a Transform")]
        public Transform transform = null;
        [Tooltip("A target, any GameObject that has a SpriteRenderer component")]
        public SpriteRenderer spriteRenderer = null;
        [Tooltip("A target, any GameObject that has a Text component")]
        public Text text = null;
        [Tooltip("The property that is being tweened")]
        public TweenType tweenType;
        [Tooltip("Determines if you are supplying both source and dest, or if you're fetching either value from the object being tweened")]
        public RangeType rangeType;
        [Tooltip("The starting value for the tween")]
        public TweenValue source;
        [Tooltip("the ending value for the tween")]
        public TweenValue dest;

        [Tooltip("Advanced Options")]
        public TweenOptions options = new TweenOptions();

        [HideInInspector]
        public TweenValue startValue; // private, used during tweening

        [HideInInspector]
        public TweenValue endValue; // private, used during tweening

        [HideInInspector]
        public float timeRemaining; // the time remaining to tween
        [HideInInspector]
        public int loopsRemaining; // the number of loops still to run

        [HideInInspector]
        public TweenValue value; // the current value of the tween

        /// <summary>
        ///  Constructs a new Tween from an existing Tween instance. Also see Clone().
        /// <para>Useful for copying a template tween</para>
        /// /// </summary>
        /// <param name="tween">The tween instance to copy</param>
        /// <param name="includeEvents">If true, shares the events from the tween being copied; if false, starts with no events</param>
        public Tween(Tween tween, bool includeEvents = false)
        {
            this.transform = tween.transform;
            this.spriteRenderer = tween.spriteRenderer;
            this.text = tween.text;
            TweenOptions options = tween.options.Clone(includeEvents);
            Initialize(tween.name, tween.duration, tween.tweenType, tween.rangeType,
                tween.source.value, tween.dest.value, options);
        }

        /// <summary>
        /// Constructor. See Tween class for parameter definitions. I'm lazy.
        /// </summary>
        public Tween(string name, float duration, Transform transform, TweenType tweenType, RangeType rangeType,
            Vector4? source = null, Vector4 ? dest = null, TweenOptions options = null)
        {
            this.transform = transform;
            Initialize(name, duration, tweenType, rangeType, source, dest, options);
        }

        /// <summary>
        /// Constructor. See Tween class for parameter definitions. I'm lazy.
        /// </summary>
        public Tween(string name, float duration, SpriteRenderer spriteRenderer, TweenType tweenType, RangeType rangeType,
            Vector4? source = null, Vector4 ? dest = null, TweenOptions options = null)
        {
            this.spriteRenderer = spriteRenderer;
            Initialize(name, duration, tweenType, rangeType, source, dest, options);
        }

        /// <summary>
        /// Constructor. See Tween class for parameter definitions. I'm lazy.
        /// </summary>
        public Tween(string name, float duration, Text text, TweenType tweenType, RangeType rangeType,
            Vector4? source = null, Vector4 ? dest = null, TweenOptions options = null)
        {
            this.text = text;
            Initialize(name, duration, tweenType, rangeType, source, dest, options);
        }

        /// <summary>
        /// Clones the tween and replaces the target the with components of the GameObject you supply
        /// </summary>
        /// <param name="go">The game object target to replace</param>
        /// <param name="includeEvents">If true, the clone shares the TweenEvents object with the Tween being cloned</param>
        /// <returns>A copy of the Tween with the target(s) replaced</returns>
        public Tween Clone(GameObject go = null, bool includeEvents = false)
        {
            Tween tween = new Tween(this, includeEvents);
            if (go != null)
            {
                tween.transform = go.transform;
                tween.spriteRenderer = go.GetComponent<SpriteRenderer>();
                tween.text = go.GetComponent<Text>();
            }
            return tween;
        }

        /// <summary>
        /// Called by Play() to prepare the Tween for updating.
        /// </summary>
        public void Activate(bool resetLoops = true)
        {
            if (duration <= 0)
                throw new UnityException("Duration must be > 0");

            switch (rangeType)
            {
                case RangeType.TargetToDest:
                    startValue = GetTargetValue();
                    endValue = options.relativeVals ? startValue + dest : dest;
                    break;

                case RangeType.SourceToDest:
                    startValue = options.relativeVals ? startValue + source : source;
                    endValue = options.relativeVals ? startValue + dest : dest;
                    break;

                case RangeType.SourceToTarget:
                    startValue = options.relativeVals ? startValue + source : source;
                    endValue = GetTargetValue();
                    break;

                case RangeType.WaitThenDest:
                    startValue = GetTargetValue();
                    endValue = options.relativeVals ? startValue + dest : dest;
                    break;
            }

            value = startValue;
            timeRemaining = duration;
            if (resetLoops)
                loopsRemaining = options.loops;
        }

        private void Initialize(string name, float duration, TweenType tweenType, RangeType rangeType,
            Vector4? source, Vector4 ? dest, TweenOptions options)
        {
            this.name = name.IsEmpty() ? "default" : name;
            this.duration = duration;
            this.tweenType = tweenType;
            this.rangeType = rangeType;
            this.source = new TweenValue(source == null ? Vector4.zero : (Vector4) source);
            this.dest = new TweenValue(dest == null ? Vector4.zero : (Vector4) dest);
            this.options = options == null ? new TweenOptions() : options;
        }

        private TweenValue GetTargetValue()
        {
            switch (tweenType)
            {
                case TweenType.SpriteRendererAlpha:
                    spriteRenderer.ThrowIfNull("SpriteRenderer not assigned");
                    return TweenValue.Float(spriteRenderer.color.a);

                case TweenType.SpriteRendererColor:
                    spriteRenderer.ThrowIfNull("SpriteRenderer not assigned");
                    return TweenValue.Color(spriteRenderer.color);

                case TweenType.TextAlpha:
                    text.ThrowIfNull("Text not assigned");
                    return TweenValue.Float(text.color.a);

                case TweenType.TextColor:
                    text.ThrowIfNull("Text not assigned");
                    return TweenValue.Color(text.color);

                case TweenType.LocalPosition2D:
                    transform.ThrowIfNull("Transform not assigned");
                    return TweenValue.Vector2(transform.localPosition);

                case TweenType.LocalPosition3D:
                    transform.ThrowIfNull("Transform not assigned");
                    return TweenValue.Vector3(transform.localPosition);

                case TweenType.Position2D:
                    transform.ThrowIfNull("Transform not assigned");
                    return TweenValue.Vector2(transform.position);

                case TweenType.Position3D:
                    transform.ThrowIfNull("Transform not assigned");
                    return TweenValue.Vector3(transform.position);

                case TweenType.LocalRotation2D:
                    transform.ThrowIfNull("Transform not assigned");
                    return TweenValue.Float(transform.localEulerAngles.z);

                case TweenType.LocalRotation3D:
                    transform.ThrowIfNull("Transform not assigned");
                    return TweenValue.Vector3(transform.localEulerAngles);

                case TweenType.Rotation2D:
                    transform.ThrowIfNull("Transform not assigned");
                    return TweenValue.Float(transform.eulerAngles.z);

                case TweenType.Rotation3D:
                    transform.ThrowIfNull("Transform not assigned");
                    return TweenValue.Vector3(transform.eulerAngles);

                case TweenType.Scale2D:
                case TweenType.Scale3D:
                    transform.ThrowIfNull("Transform not assigned");
                    return TweenValue.Vector3(transform.localScale);

                default:
                    throw new UnityException("TweenType" + tweenType + " does not have an object value");
            }
        }
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
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [System.Serializable]
    public class TweenOptions
    {
        [Tooltip("The number of times to play the tween; use -1 for infinite looping")]
        public int loops;

        [Tooltip("If true, source/dest values are interpreted relative to the target")]
        public bool relativeVals;

        [Tooltip("If true, plays tween in reverse (1-0 instead of 0-1)")]
        public bool pingPong;

        [Tooltip("If true, plays tween in reverse (1-0 instead of 0-1)")]
        public bool reverse;

        [Tooltip("An optional easing curve")]
        public AnimationCurve easing;

        [Tooltip("Optional event notifications")]
        public TweenEvents events = new TweenEvents();

        public TweenOptions() { }

        public TweenOptions(TweenOptions options, bool includeEvents = false)
        {
            loops = options.loops;
            relativeVals = options.relativeVals;
            pingPong = options.pingPong;
            reverse = options.reverse;
            easing = options.easing;

            if (includeEvents)
                events = options.events;
        }

        public TweenOptions Clone(bool includeEvents = false)
        {
            return new TweenOptions(this, includeEvents);
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

        /// <returns>2D rotation angle, alpha or raw float</returns>
        public float Float()
        {
            return value.x;
        }

        /// <returns>2D position and scale</returns>
        public Vector2 Vector2()
        {
            return (Vector2) value;
        }

        /// <returns>All 3D tween types</returns>
        public Vector3 Vector3()
        {
            return (Vector3) value;
        }

        /// <returns>The native Vector4</returns>
        public Vector3 Vector4()
        {
            return value;
        }

        /// <returns>Colors</returns>
        public Color Color()
        {
            return (Color) value;
        }

        public static TweenValue Float(float f)
        {
            return new TweenValue(f, 0, 0, 0);
        }

        public static TweenValue Vector2(Vector2 v)
        {
            return new TweenValue((Vector4) v);
        }

        public static TweenValue Vector3(Vector3 v)
        {
            return new TweenValue((Vector4) v);
        }

        public static TweenValue Vector4(Vector4 v)
        {
            return new TweenValue(v);
        }

        public static TweenValue Color(Color c)
        {
            return new TweenValue((Vector4) c);
        }

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
    public enum TweenType
    {
        Position3D,
        Position2D,
        LocalPosition2D,
        LocalPosition3D,
        Rotation2D,
        Rotation3D,
        LocalRotation2D,
        LocalRotation3D,
        Scale2D,
        Scale3D,
        SpriteRendererColor,
        SpriteRendererAlpha,
        TextColor,
        TextAlpha,
        Float,
        Vector2,
        Vector3,
        Vector4,
        Color,
        None
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [System.Serializable]
    public enum RangeType
    {
        SourceToDest,
        TargetToDest,
        SourceToTarget,
        WaitThenDest,
        JustWait
    }

#if UNITY_EDITOR

    //////////////////////////////////////////////////////////////////////////////// 

    [CustomPropertyDrawer(typeof (Tween))]
    public class TweenPropertyDrawer : PropertyDrawer
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
                return EditorGUI.GetPropertyHeight(prop) + 20f;
            return EditorGUI.GetPropertyHeight(prop);
        }
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [CustomPropertyDrawer(typeof (TweenValue))]
    public class TweenValuePropertyDrawer : PropertyDrawer
    {
        private float lastX;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if (IsHidden(prop)) return;

            string tweenPath = prop.propertyPath.Substring(0, prop.propertyPath.LastIndexOf("."));
            string tweenTypePath = tweenPath + ".tweenType";
            SerializedProperty tweenTypeProp = prop.serializedObject.FindProperty(tweenTypePath);

            TweenType tweenType = (TweenType) tweenTypeProp.enumValueIndex;
            EditorGUI.BeginProperty(pos, new GUIContent("Target"), prop);
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            SerializedProperty value = prop.FindPropertyRelative("value");
            lastX = 30;

            AddLabel(value, pos, prop.displayName, 90f);
            switch (tweenType)
            {
                case TweenType.LocalRotation2D:
                case TweenType.Rotation2D:
                AddField(value, pos, "x", 100f, 35f, "angle");
                break;

                case TweenType.Float:
                AddField(value, pos, "x", 100f, 35f, "value");
                break;

                case TweenType.SpriteRendererAlpha:
                case TweenType.TextAlpha:
                AddField(value, pos, "x", 100f, 35f, "alpha");
                break;

                case TweenType.LocalPosition2D:
                case TweenType.Position2D:
                case TweenType.Scale2D:
                case TweenType.Vector2:
                AddField(value, pos, "x", 90f, 10f);
                AddField(value, pos, "y", 90f, 10f);
                break;

                case TweenType.LocalPosition3D:
                case TweenType.Position3D:
                case TweenType.LocalRotation3D:
                case TweenType.Rotation3D:
                case TweenType.Scale3D:
                case TweenType.Vector3:
                AddField(value, pos, "x", 65f, 10f);
                AddField(value, pos, "y", 65f, 10f);
                AddField(value, pos, "z", 65f, 10f);
                break;

                case TweenType.SpriteRendererColor:
                case TweenType.TextColor:
                case TweenType.Color:
                AddField(value, pos, "x", 46f, 10f, "r");
                AddField(value, pos, "y", 46f, 10f, "g");
                AddField(value, pos, "z", 46f, 10f, "b");
                AddField(value, pos, "w", 46f, 10f, "a");
                break;

                default: // Vector4
                AddField(value, pos, "x", 40f);
                AddField(value, pos, "y", 40f);
                AddField(value, pos, "z", 40f);
                AddField(value, pos, "w", 40f);
                break;
            }

            EditorGUI.indentLevel = indentLevel;
            EditorGUI.EndProperty();
        }
        private void AddLabel(SerializedProperty prop, Rect pos, string propName, float width)
        {
            Rect fieldRect = new Rect(pos.x + lastX, pos.y, width, pos.height);
            EditorGUI.LabelField(fieldRect, propName);
            lastX += width;
        }

        private void AddField(SerializedProperty prop, Rect pos, string propName, float width,
            float labelWidth = 10f, string altLabel = null)
        {
            string label = (altLabel == null ? propName : altLabel);
            SerializedProperty fieldProp = prop.FindPropertyRelative(propName);
            Rect fieldRect = new Rect(pos.x + lastX, pos.y, width, pos.height);
            EditorGUI.LabelField(fieldRect, label);
            fieldRect.x += labelWidth;
            fieldRect.width = width - labelWidth;
            EditorGUI.PropertyField(fieldRect, fieldProp, GUIContent.none);
            lastX += width + 5;
        }

        private bool IsHidden(SerializedProperty prop)
        {
            string tweenPath = prop.propertyPath.Substring(0, prop.propertyPath.LastIndexOf("."));
            string rangeTypePath = tweenPath + ".rangeType";
            SerializedProperty rangeTypeProp = prop.serializedObject.FindProperty(rangeTypePath);

            RangeType rangeType = (RangeType) rangeTypeProp.enumValueIndex;
            return (rangeType == RangeType.SourceToTarget && prop.name == "dest") ||
                (rangeType == RangeType.TargetToDest && prop.name == "source") ||
                (rangeType == RangeType.WaitThenDest && prop.name == "source") ||
                rangeType == RangeType.JustWait;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            if (IsHidden(prop)) return -EditorGUIUtility.standardVerticalSpacing;
            return EditorGUI.GetPropertyHeight(prop, label, false);
        }
    }

    //////////////////////////////////////////////////////////////////////////////// 

    [CustomPropertyDrawer(typeof (Transform))]
    [CustomPropertyDrawer(typeof (SpriteRenderer))]
    [CustomPropertyDrawer(typeof (Text))]
    public class TargetPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if (IsHidden(prop)) return;
            EditorGUI.PropertyField(pos, prop, new GUIContent("Target"));
        }

        private bool IsHidden(SerializedProperty prop)
        {
            string tweenPath = prop.propertyPath.Substring(0, prop.propertyPath.LastIndexOf("."));
            SerializedProperty rangeTypeProp = prop.serializedObject.FindProperty(tweenPath + ".rangeType");
            SerializedProperty tweenTypeProp = prop.serializedObject.FindProperty(tweenPath + ".tweenType");

            switch ((TweenType) tweenTypeProp.enumValueIndex)
            {
                case TweenType.Float:
                case TweenType.Vector2:
                case TweenType.Vector3:
                case TweenType.Vector4:
                case TweenType.Color:
                rangeTypeProp.enumValueIndex = (int) RangeType.SourceToDest; // these types have no target 
                return true; // plain jane values do not use a target

                case TweenType.SpriteRendererColor:
                case TweenType.SpriteRendererAlpha:
                return prop.name != "spriteRenderer";

                case TweenType.TextColor:
                case TweenType.TextAlpha:
                return prop.name != "text";

                case TweenType.None:
                rangeTypeProp.enumValueIndex = (int) RangeType.JustWait;
                return true;
            }

            // The remainder are all transforms
            return prop.name != "transform";
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            if (IsHidden(prop)) return -EditorGUIUtility.standardVerticalSpacing;
            return EditorGUI.GetPropertyHeight(prop, label, false);
        }

    }
#endif
}