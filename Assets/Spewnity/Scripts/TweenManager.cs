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
// TODO Is it possible to clone a UnityEvent? It might not be necessary but I'd like to be able to clone the TweenEvents object properly.
// TODO Disabled properties do not show their tooltips
namespace Spewnity
{
    /// <summary>
    /// Another tweening system.
    /// <para>You can specify persistant tweens in the inspector or ad-hoc ones through the API. Persistent tweens can be run 
    /// as defined with Play(string), or they can be used as templates: Clone() and modify the tween, and then Play(tween).</para>
    /// <para>The tweens system supports transform tweening, color tweening on certain components, and restricting certain axes.
    /// Also, freely tween independent floats, vectors, and colors using the event system. Supports easing, ping-pong, 
    /// reverse, and relative tween values.</para>
    /// <para>Preview your tweens live, just by clicking the button in the inspector while playing.</para>
    /// <para>Hook in events for tween start, change, and stop. Use Compound Tweens to choreograph several tweens with 
    /// different objects together.</para>
    /// <para>You can also use the Tween class directly, bypassing TweenManager. Call Activate() to start your tween, and Process() 
    /// it during your Update method</para>
    /// </summary>
    public class TweenManager : MonoBehaviour
    {
        public static TweenManager instance;

        [Tooltip("If you have multiple TweenManagers, setting this to true ensures this TweenManager is assigned to TweenManager.instance")]
        public bool primaryInstance;

        [Tooltip("A set of one or more persistant tweens, that can also be used as tween templates")]
        public List<Tween> tweenTemplates;

        [Tooltip("Each TweenSchedule references a set of tweens that play at the scheduled time, in concert")]
        public List<CompoundTween> compoundTweens;

        private List<Tween> activeTweens = new List<Tween>();
        private List<Tween> tweensToAdd = new List<Tween>();
        private List<CompoundTween> activeCompounds = new List<CompoundTween>();

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

            if (activeTweens.Contains(tween))
                activeTweens.Remove(tween);

            if (tweensToAdd.Contains(tween))
                tweensToAdd.Remove(tween);

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
            foreach (List<Tween> list in new List<Tween>[] { activeTweens, tweensToAdd })
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
            activeTweens.Remove(tween);
        }

        /// <summary>
        /// Stops all tweens from running.
        /// </summary>
        public void StopAll()
        {
            activeTweens.Clear();
        }

        /// <summary>
        /// Determines if a tween is playing, based on its name.
        /// <para>If the name is not unique, this may return a false positive. See IsPlaying(Tween).</para>
        /// </summary>
        /// <param name="tweenName">The name of the tween</param>
        /// <returns>True if the tween is playing now or immediately</returns>
        public bool IsPlaying(string tweenName)
        {
            return activeTweens.Exists(tween => tween.name == tweenName) || tweensToAdd.Exists(tween => tween.name == tweenName);
        }

        /// <summary>
        /// Determines if a tween instance is playing.
        /// </summary>
        /// <param name="tweenName">The tween instance</param>
        /// <returns>True if the tween is playing now or immediately</returns>
        public bool IsPlaying(Tween tween)
        {
            return activeTweens.Contains(tween) || tweensToAdd.Contains(tween);
        }

        public CompoundTween GetCompound(string name)
        {
            CompoundTween compound = compoundTweens.Find(x => x.name == name);
            if (compound == null)
                throw new KeyNotFoundException("CompoundTween not found:" + name);
            return compound;
        }

        public void PlayCompound(string name)
        {
            PlayCompound(GetCompound(name));
        }

        public void PlayCompound(CompoundTween compound)
        {
            compound.Activate(this);
            activeCompounds.Add(compound);
        }

        /// <summary>
        /// Determines if a tween is playing, based on its name, and returns it.
        /// </summary>
        /// <param name="tweenName">The name of the tween</param>
        /// <returns>A tween which is playing now or immediately, or null if no such tween is found</returns>
        public Tween GetPlaying(string tweenName)
        {
            Tween tween = activeTweens.Find(t => t.name == tweenName);
            if (tween == null) tweensToAdd.Find(t => t.name == tweenName);
            return tween;
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
            ProcessCompounds();
            ProcessActiveTweens();
            ProcessNewTweens();
        }

        public void ProcessCompounds()
        {
            for (int i = activeCompounds.Count - 1; i >= 0; i--)
            {
                CompoundTween compound = activeCompounds[i];
                if (compound.Apply(this) == true)
                    activeCompounds.RemoveAt(i);
            }
        }

        public void ProcessActiveTweens()
        {
            // Process all active tweens
            foreach (Tween tween in activeTweens)
                tween.Process();

            // Remove any inactive tweens from list
            activeTweens.RemoveAll(t => t.loopsRemaining == 0);
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
                activeTweens.Add(tween);
                tween.Activate(gameObject);
            }
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
        [Tooltip("The unique name of the tween; required for template tweens defined in TweenManager")]
        public string name;

        [Tooltip("The duration of the tween in seconds, must be > 0")]
        public float duration;

        [Tooltip("The GameObject to tween; if you are doing color/alpha tweening, this must contain a SpriteRenderer, Rendererer, Text, TextMesh, or TextMeshPro; if using TweenManager, this can be null")]
        public GameObject target;

        [Tooltip("Adjusts the position of the transform")]
        public TweenPropertyPosition position;

        [Tooltip("Adjusts the rotation of the transform, in degrees")]
        public TweenPropertyRotation rotation;

        [Tooltip("Adjusts the scale of the transform; note that 1.0 is default size")]
        public TweenPropertyScale scale;

        [Tooltip("Adjusts the color of a SpriteRenderer, Rendererer, Text, TextMesh, or TextMeshPro component attached to this GameObject")]
        public TweenPropertyColor color;

        [Tooltip("The value of the the tween after it is kicked off, or the last value assigned to the target after the tween finished")]
        public TweenPropertyValue value;

        [Tooltip("Advanced Options")]
        public TweenOptions options;

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
        public Component component;

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
        public Tween(Tween tween, GameObject go = null, bool includeEvents = false)
        {
            TweenOptions newOptions = tween.options.Clone(includeEvents);
            GameObject newTarget = (go == null ? tween.target : go);
            Init(tween.name, tween.duration, newTarget, tween.position, tween.rotation, tween.scale, tween.color, tween.value, newOptions);
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
            return new Tween(this, go, includeEvents);
        }

        /// <summary>
        /// Called when a tween is looping.
        /// </summary>
        public void Reactivate()
        {
            Activate(null, true);

        }

        /// <summary>
        /// Prepare the tween for updating. Usually the first step! See Apply().
        /// If using TweenManager, this will be done for you.
        /// </summary>
        /// <param name="autoTarget">If set, will use as the default target when target is null; if using TweenManager, it will offer itself in this role</param>
        /// <param name="reactivatingFromLoop">Should be set to true when looping</param>
        public void Activate(GameObject autoTarget = null, bool reactivatingFromLoop = false)
        {
            if (duration <= 0)
                throw new UnityException("Duration must be > 0");

            // Supply default target if requested
            if (this.target == null)
            {
                if (autoTarget != null)
                    this.target = autoTarget;
                else if (position.enabled || rotation.enabled || scale.enabled || color.enabled)
                    throw new UnityException("Target for tween '" + name + "' is missing.");
            }

            // Update caches for non-transform components
            this.component = null;
            if (target != null)
            {
                if (component == null) component = target.GetComponent<TextMesh>();
                if (component == null) component = target.GetComponent<Text>();
                if (component == null) component = target.GetComponent<SpriteRenderer>();
                if (component == null) component = target.GetComponent("TMPro.TextMeshPro");
                if (component == null) component = target.GetComponent<Renderer>();
            }

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

            if (options.events != null)
                options.events.Start.Invoke(this);
            Apply();
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

        /// <summary>
        /// Call this regularly during Update() to continue this tween.
        /// If using TweenManager, this will be done for you.
        /// </summary>
        public void Process()
        {
            if (loopsRemaining == 0)
                return;

            // Tween loop has run its delay
            if (delayRemaining > 0)
            {
                delayRemaining -= Time.deltaTime;
                if (delayRemaining > 0)
                    return;
                delayRemaining = 0f;
                options.events.Loop.Invoke(this);
            }

            // Tween loop has run its duration
            else
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining < 0f)
                {
                    timeRemaining = 0f;
                    loopsRemaining--;
                }
            }

            // Apply Tween changes
            Apply();

            if (timeRemaining <= 0f)
            {
                // Tween has finished loop
                if (loopsRemaining != 0)
                    Reactivate();

                // has finished all iterations
                else options.events.End.Invoke(this);
            }
        }

        private void Apply()
        {
            float timeRatio = timeRemaining / duration;

            foreach (TweenProperty tp in new TweenProperty[] { position, rotation, scale, color, value })
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
                if (tp.range == TweenPropertyRange.Dest && timeRemaining > 0f && loopsRemaining != 0)
                    continue;

                // Apply tween to object
                tp.Apply(this);
            }

            // Call change event
            if (options.events != null)
                options.events.Change.Invoke(this);
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
            if (tween.component is SpriteRenderer)
                return TweenValue.Color((tween.component as SpriteRenderer).color);
            else if (tween.component is Text)
                return TweenValue.Color((tween.component as Text).color);
            else if (tween.component is Renderer)
                return TweenValue.Color((tween.component as Renderer).material.color);
            else if (tween.component is TextMesh)
                return TweenValue.Color((tween.component as TextMesh).color);
            else if (tween.component.GetType().ToString() == "TMPro.TextMeshPro") // TextMeshPro may not be installed
                return TweenValue.Color((Color) tween.component.GetType().GetProperty("color").GetValue(tween.component, null));
            else throw new UnityException("TweenType.Color requires target to have a SpriteRenderer, Renderer, Text, TextMesh or TextMeshPro component (Found:" +
                tween.component.GetType().ToString() + ")");
        }

        public override void Apply(Tween tween)
        {
            if (tween.component is SpriteRenderer)
                ((SpriteRenderer) tween.component).color = value.Color();
            else if (tween.component is Text)
                ((Text) tween.component).color = value.Color();
            else if (tween.component is Renderer)
                ((Renderer) tween.component).material.color = value.Color();
            else if (tween.component is TextMesh)
                ((TextMesh) tween.component).color = value.Color();
            else if (tween.component != null && tween.component.GetType().ToString() == "TMPro.TextMeshPro") // TextMeshPro may not be installed
                tween.component.GetType().GetProperty("color").SetValue(tween.component, value.Color(), null);
            else throw new UnityException("TweenType.Color requires target to have a SpriteRenderer, Renderer, Text, TextMesh or TextMeshPro component (Found:" +
                tween.component.GetType().ToString() + ")");
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
    public class CompoundTween
    {
        [Tooltip("The name of this schedule; suitable for passing to PlayCompound()")]
        public string name;

        [Tooltip("One or more tasks, that consists of a tween to play and when to play it")]
        public List<CompoundTweenTask> tasks = new List<CompoundTweenTask>();

        public CompoundTween(string name, List<CompoundTweenTask> tasks = null)
        {
            this.name = name;
            if (tasks != null)
                this.tasks = tasks;
        }

        public CompoundTween(CompoundTween ct)
        {
            this.name = ct.name;
            foreach (CompoundTweenTask task in tasks)
                this.tasks.Add(task.Clone());
        }

        public CompoundTween Clone()
        {
            return new CompoundTween(this);
        }

        public void Activate(TweenManager tm)
        {
            foreach (CompoundTweenTask task in tasks)
                task.Activate(tm);
        }

        public bool Apply(TweenManager tm)
        {
            bool complete = true;

            foreach (CompoundTweenTask task in tasks)
                if (task.Apply(tm) == false)
                    complete = false;

            return complete;
        }
    }

    [System.Serializable]
    public class CompoundTweenTask
    {
        [Tooltip("The name of a template tween to play")]
        public string template;

        [Tooltip("Once the main tween plays, the amount to wait (in seconds) before playing this subtween; usually the first is set to 0")]
        public float delay;

        [System.NonSerialized]
        public Tween tween; // A tween assigned dynamically

        [HideInInspector]
        public float timeRemaining;

        [System.NonSerialized]
        public bool active = false;

        public CompoundTweenTask(CompoundTweenTask task, GameObject altTarget = null)
        {
            this.template = task.template;
            this.delay = task.delay;
            this.tween = tween == null ? null : tween.Clone(altTarget);
        }

        public CompoundTweenTask(CompoundTweenTask task, TweenManager tm, GameObject altTarget)
        {
            this.template = task.template;
            this.delay = task.delay;
            this.tween = tween == null ? tm.GetTemplate(this.template) : tween.Clone(altTarget);
        }

        public CompoundTweenTask Clone(GameObject altTarget = null)
        {
            return new CompoundTweenTask(this, altTarget);
        }

        public CompoundTweenTask Clone(TweenManager tm, GameObject altTarget)
        {
            return new CompoundTweenTask(this, tm, altTarget);
        }

        public CompoundTweenTask(string template, float delay)
        {
            this.template = template;
            this.delay = delay;
            this.tween = null;
        }

        public CompoundTweenTask(Tween tween, float delay)
        {
            tween.ThrowIfNull();
            this.tween = tween;
            this.template = tween.name;
            this.delay = delay;
        }

        public void Activate(TweenManager tm)
        {
            if (this.tween == null)
                this.tween = tm.GetTemplate(template);
            this.timeRemaining = this.delay;
            this.active = true;
        }

        public bool Apply(TweenManager tm)
        {
            if (!active)
                return true;

            this.timeRemaining -= Time.deltaTime;
            if (this.timeRemaining > 0)
                return false;

            this.timeRemaining = 0;
            tm.Play(this.tween);
            active = false;
            return true;
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

    [CustomPropertyDrawer(typeof (CompoundTween))]
    public class CompoundTweenPD : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if (prop == null)
                return;

            EditorGUI.PropertyField(pos, prop, label, true);
            if (prop.isExpanded)
            {
                GUI.enabled = Application.isPlaying;
                float x = Helper.GetControlLeft(pos.width);
                if (GUI.Button(new Rect(pos.x + x, pos.yMax - 25f, 100f, 20f),
                        new GUIContent("Live Preview", "When application is running, press this to preview the compound tween")))
                {
                    TweenManager tm = (TweenManager) prop.serializedObject.targetObject;
                    string name = prop.serializedObject.FindProperty(prop.propertyPath + ".name").stringValue;
                    tm.StopAll();
                    tm.PlayCompound(name);
                }
                GUI.enabled = true;
            }
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(prop);

            if (prop.isExpanded)
                height += 30f;

            return height;
        }
    }

    [CustomPropertyDrawer(typeof (CompoundTweenTask))]
    public class CompoundTweenTaskPD : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if (prop == null)
                return;

            EditorGUI.BeginProperty(pos, label, prop);
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel - 2;
            Helper.lastX = 30f;
            Helper.AddLabel(pos, "Task", 90f);
            EditorGUI.indentLevel = 0;
            Helper.lastX = Helper.GetControlLeft(pos.width);
            float width = pos.width - Helper.lastX;

            TweenManager tm = (TweenManager) prop.serializedObject.targetObject;
            SerializedProperty templateProp = prop.FindPropertyRelative("template");
            string template = templateProp.stringValue;
            List<string> values = new List<string>();
            int selected = -1;
            for (int i = 0; i < tm.tweenTemplates.Count; i++)
            {
                string name = tm.GetTemplateByIndex(i).name;
                values.Add(name);
                if (name == template)
                    selected = i + 1;
            }

            string msg = "Select a template";
            if (selected == -1)
            {
                if (!template.IsEmpty())
                    msg = "Missing template";
                selected = 0;
            }
            values.Insert(0, msg);

            float delayWidth = pos.width > 320f ? width * 0.4f : width * 0.3f;
            string delayLabel = pos.width > 320f ? "delay" : "d";

            selected = Helper.AddPopup(values, selected, pos, width - delayWidth, false);
            templateProp.stringValue = selected > 0 ? values[selected] : "";
            Helper.AddField(prop, pos, "delay", delayWidth, false, delayLabel.Length > 1 ? 30f : 10f, delayLabel);

            EditorGUI.indentLevel = indentLevel;
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof (Tween))]
    public class TweenPD : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            // Alternating background colors
            int index = Helper.GetTweenIndex(prop);
            if (index > -1)
                EditorGUI.DrawRect(pos, new Color(0f, 0f, 0f, index % 2 == 0 ? 0.1f : 0f));

            EditorGUI.PropertyField(pos, prop, label, true);

            if (prop.isExpanded && prop.GetParent() is TweenManager)
            {
                GUI.enabled = Application.isPlaying;
                float x = Helper.GetControlLeft(pos.width);
                if (GUI.Button(new Rect(pos.x + x, pos.yMax - 25f, 100f, 20f),
                        new GUIContent("Live Preview", "When Tween Manager is running, press this to preview the tween")))
                {
                    TweenManager tm = (TweenManager) prop.serializedObject.targetObject;
                    string name = prop.serializedObject.FindProperty(prop.propertyPath + ".name").stringValue;
                    tm.Stop(name);
                    tm.Play(name);
                }
                GUI.enabled = true;
            }

            //     EditorGUI.Popup(new Rect(pos.xMin + (pos.width - 150f) / 2, pos.yMax - 40f, 150f, 20f),
            //         0, new string[] { "hi", "there", "would you like to ride", "the bone train?" });
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(prop);
            // height += EditorGUIUtility.singleLineHeight; // POPUP TEST 

            if (prop.isExpanded)
                height += 30f;

            return height;
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
            TweenProperty tp = prop.GetObject() as TweenProperty;
            if (tp == null)
                return;

            // A little bit of color to distinguish the sections
            Color color = GUI.backgroundColor;
            if (tp is TweenPropertyPosition)
                GUI.backgroundColor = new Color(1f, .9f, .9f);
            else if (tp is TweenPropertyRotation)
                GUI.backgroundColor = new Color(.9f, 1f, .9f);
            else if (tp is TweenPropertyScale)
                GUI.backgroundColor = new Color(.9f, .9f, 1f);
            else if (tp is TweenPropertyColor)
                GUI.backgroundColor = new Color(1f, 1f, .9f);
            else GUI.backgroundColor = new Color(.9f, 1f, 1f);

            if (!tp.enabled)
                EditorGUI.LabelField(pos, prop.displayName);

            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float x = Helper.GetControlLeft(pos.width);
            Rect r = new Rect(pos.x + x, pos.y, pos.width - x, EditorGUIUtility.singleLineHeight);
            bool wasEnabled = tp.enabled;
            tp.enabled = EditorGUI.ToggleLeft(r, "Enabled", tp.enabled);
            EditorGUI.indentLevel = indentLevel;

            if (tp.enabled && !wasEnabled)
                prop.isExpanded = true;

            if (tp.enabled)
                EditorGUI.PropertyField(pos, prop, new GUIContent(prop.displayName), true);

            GUI.backgroundColor = color;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            TweenProperty tp = prop.GetObject() as TweenProperty;
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

            TweenProperty tp = prop.GetParent() as TweenProperty;
            SerializedProperty value = prop.FindPropertyRelative("value");

            EditorGUI.BeginProperty(pos, new GUIContent("Target"), prop);
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel - 2;
            Helper.lastX = 30f;
            Helper.AddLabel(pos, prop.displayName, 90f);
            EditorGUI.indentLevel = 0;
            Helper.lastX = Helper.GetControlLeft(pos.width);
            float adjustedWidth = pos.width - Helper.lastX;
            tp.AddValueFields(value, pos, adjustedWidth, true);
            EditorGUI.indentLevel = indentLevel;
            EditorGUI.EndProperty();
        }
        private bool IsHidden(SerializedProperty prop)
        {
            TweenProperty tp = prop.GetParent() as TweenProperty;
            if (tp == null)
                return true;

            return (tp.range == TweenPropertyRange.SourceToTarget && prop.name == "dest") ||
                (tp.range == TweenPropertyRange.TargetToDest && prop.name == "source") ||
                (tp.range == TweenPropertyRange.Dest && prop.name == "source");
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
            TweenProperty tp = prop.GetParent() as TweenProperty;
            if (tp == null || (tp is TweenPropertyValue && ((TweenPropertyValue) tp).type == TweenPropertyValueType.Float))
                return;

            EditorGUI.BeginProperty(pos, label, prop);
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel - 2;
            Helper.lastX = 30f;
            Helper.AddLabel(pos, pos.width > 360 ? "Frozen Axes" : "Frozen", 90f);
            EditorGUI.indentLevel = 0;
            Helper.lastX = Helper.GetControlLeft(pos.width);
            float adjustedWidth = pos.width - Helper.lastX;
            tp.AddValueFields(prop, pos, adjustedWidth);
            EditorGUI.indentLevel = indentLevel;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            TweenProperty tp = prop.GetParent() as TweenProperty;
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

        public static int GetTweenIndex(SerializedProperty prop)
        {
            Match match = Regex.Match(prop.propertyPath, @"\.data\[(\d+)\]$");
            if (match.Groups.Count < 2)
                return -1;
            return int.Parse(match.Groups[1].Value);
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

        public static int AddPopup(List<string> values, int selected, Rect pos, float width, bool isDisabled = false)
        {
            Rect fieldRect = new Rect(pos.x + Helper.lastX, pos.y, width - 5f, pos.height);
            Helper.lastX += width;
            return EditorGUI.Popup(fieldRect, selected, values.ToArray());
        }

        public static float GetControlLeft(float propertyWidth)
        {
            return (propertyWidth < 338f ? EditorGUIUtility.labelWidth : (propertyWidth - 338f) * 0.45f + EditorGUIUtility.labelWidth);
        }
    }

    // https://issuetracker.unity3d.com/issues/propertydrawer-editorgui-dot-drawrect-disappears-when-unfocused
    [CanEditMultipleObjects][CustomEditor(typeof (MonoBehaviour), true)] public class MonoBehaviour_DummyCustomEditor : Editor { }
#endif
}