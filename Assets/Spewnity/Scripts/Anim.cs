using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

/**
 * A lightweight 2D animator.
 *
 * Drag sprite frames to the frames array (you may have to lock the Anim's GameObject in the inspector).
 * Then specify animations with a unique name and frames. The frame definition consists comma-separated
 * values, such as "0-9,9x10,9-0". Each value must be in one of the formats:
 * 
 *     value - a single integer (corresponding to the index in the frames array, such as "3") 
 *     range - a min-max or max-min range with a hyphen between the values ("5-10"); the extents are inclusive
 *     reps  - a value to be repeated and a number of reps, separated by an x ("0x10")
 *
 * To specify the initial animation, supply a sequenceName matching the name of one of your sequences.
 * To change the sequence at runtime, call Play(). To view your initial animation in real time, check
 * Live Preview. Note that the live preview may run slower than your normal frame rate. The GameObject 
 * must have a SpriteRenderer attached.
 *
 * The animation will automatically loop. You can subscribe to the onLoop event in order to stop the
 * animation (see Pause and Clear), change the current sequence (see Play), or anything else.
 * TODO: Looping should be an optional setting
 */
namespace Spewnity
{
    [ExecuteInEditMode]
    public class Anim : MonoBehaviour
    {
        public bool livePreview;
        public string sequenceName;
        public List<AnimSequence> sequences;
        public List<Sprite> frames;
        public bool paused = false;
        public AnimEvent onLoop;

        private int frame = 0;
        private AnimSequence sequence;
        private float elapsed = 0;
        private Dictionary<string, AnimSequence> cache;
        private SpriteRenderer sr;

        public void Awake()
        {
            UpdateCache();

            // Ensure there is a default SpriteRenderer
            sr = GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = gameObject.AddComponent<SpriteRenderer>();
#if DEBUG
                Debug.Log("DEBUG: No SpriteRenderer defined.");
#endif             
                if (sequenceName == "")
                    paused = true;
            }
        }

        public void Start()
        {
            Replay(sequenceName, paused);
        }

        public void OnValidate()
        {
            UpdateCache();

            if (livePreview && (sequence == null || sequence.name != sequenceName))
                Replay(sequenceName);
        }

        public void Play(string name, bool startPaused = false)
        {
            if (sequenceName == name)
                return;

            Replay(name);
        }

        public void Replay(string name, bool startPaused = false)
        {
            if (name == null || name == "")
            {
                Clear();
                return;
            }

            if (!IsCached(name))
            {
                Debug.Log(transform.GetFullPath() + "<Anim> cache failed hit for sequence name: " + name);
                return;
            }

            sequence = cache[name];
            sequenceName = name;
            frame = 0;
            elapsed = 0;
            paused = startPaused;
            UpdateView();
        }

        public bool IsCached(string name)
        {
            return cache.ContainsKey(name);
        }

        public void Freeze(string name)
        {
            Play(name);
            Pause();
        }

        // Pauses the animation on the current frame
        // Will stay paused until you call Resume() or Play() another animation
        public void Pause()
        {
            paused = true;
        }

        // Resumes a paused animation
        public void Resume()
        {
            paused = false;
        }

        // Stops and clears the playing animation
        public void Clear()
        {
            sequence = null;
            sr.sprite = null;
        }

        // Known Issue: This is a non-frame-skipping implementation. 
        // If the FPS is high enough, it will max out displaying 1 frame
        // per update call, and higher FPS will not change that.
        public void Update()
        {
            if (paused)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying && !livePreview)
                return;
#endif

            if (sequence == null)
                return;

            elapsed += Time.deltaTime;
            if (elapsed >= sequence.deltaTime)
            {
                elapsed -= sequence.deltaTime;
                if (++frame >= sequence.frameArray.Count)
                {
                    frame = 0;
                    onLoop.Invoke(this);
                }
                UpdateView();
            }
        }

        private void UpdateView()
        {
            if (sr == null)
                return;

            if (sequence == null || frames.Count == 0 || frame < 0 || frame >= sequence.frameArray.Count)
            {
                sr.sprite = null;
                return;
            }

            int cel = sequence.frameArray[frame];
            sr.sprite = frames[cel];
        }

        public AnimSequence Add(AnimSequence seq)
        {
            sequences.Add(seq);
            UpdateCache();
            return seq;
        }

        public AnimSequence Add(string name, string frames, int fps)
        {
            AnimSequence seq = new AnimSequence(name, frames, fps);
            this.Add(seq);
            return seq;
        }

        /*
         *  If you modify the sequences, frames, or fps directly, you must call 
         * UpdateCache() afterwards to set up the cache and precalculations.
         */
        public void UpdateCache()
        {
            // Recreate cache
            cache = new Dictionary<string, AnimSequence>();
            foreach (AnimSequence seq in sequences)
                cache.Add(seq.name, seq);

            // Preprocess sequence frames and fps
            foreach (AnimSequence seq in sequences)
            {
                seq.deltaTime = 1f / (float) seq.fps;
                if (seq.deltaTime <= 0)
                    Debug.Log("Illegal fps:" + seq.fps);
                seq.frameArray = GetFramesArray(seq.frames);
            }
        }

        // Converts a frame string into an array
        public static List<int> GetFramesArray(string frames)
        {
            List<int> result = new List<int>();
            string str = frames.Replace(" ", "");
            foreach (string element in str.ToLower().Split(','))
            {
                if (element.Contains("-"))
                {
                    string[] extents = element.Split('-');
                    Debug.Assert(extents.Length == 2);
                    int left = int.Parse(extents[0]);
                    int right = int.Parse(extents[1]);
                    int order = (right > left ? 1 : -1);
                    for (int i = left; i != right + order; i += order)
                        result.Add(i);
                }
                else if (element.Contains("x"))
                {
                    string[] extents = element.Split('x');
                    Debug.Assert(extents.Length == 2);
                    int val = int.Parse(extents[0]);
                    int repetition = int.Parse(extents[1]);
                    while (repetition-- > 0)
                        result.Add(val);
                }
                else
                {
                    int val = int.Parse(element);
                    result.Add(val);
                }
            }

            return result;
            // Debug.Log("Converted:" + seq.frames + " to " + seq.frameArray.Join());
        }
    }

    [System.Serializable]
    public class AnimSequence
    {
        public string name;

        [TooltipAttribute("Comma separated list of frames and ranges, e.g: 1-7,9,12-10")]
        public string frames;
        public float fps = 30;

        [HideInInspector]
        public List<int> frameArray; // frame string expanded to array
        [HideInInspector]
        public float deltaTime;

        public AnimSequence(string name, string frames, int fps)
        {
            this.name = name;
            this.frames = frames;
            this.fps = fps;
        }
    }

    [System.Serializable]
    public class AnimEvent : UnityEvent<Anim> { }

#if UNITY_EDITOR
    [CustomEditor(typeof (Anim))]
    public class AnimEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // Support live preview
            Anim anim = (Anim) target;
            if (anim.livePreview)
                EditorUtility.SetDirty(target);
        }
    }

    [CustomPropertyDrawer(typeof (AnimSequence))]
    public class AnimSequenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.PropertyField(new Rect(pos.x, pos.y, 80, pos.height), prop.FindPropertyRelative("name"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(pos.x + 85, pos.y, 50, pos.height), prop.FindPropertyRelative("fps"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(pos.x + 140, pos.y, pos.width - 140, pos.height), prop.FindPropertyRelative("frames"), GUIContent.none);
            EditorGUI.indentLevel = indentLevel;
            EditorGUI.EndProperty();
        }
    }
#endif
}