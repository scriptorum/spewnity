using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

/**
 * A lightweight 2D animator.
 * Drag sprite frames to the frames array (you may have to lock the Anim's GameObject in the inspector).
 * Then specify animations with a unique name and frames. The frame definition consists of frame numbers 
 * (corresponding to the frames array) that are comma-separated. The definition may also include ranges
 * in the form of min-max (inclusive).
 * 
 * To specify the initial animation, supply a sequenceName matching the name of one of your sequences.
 * To change the sequence at runtime, call Play(). To view your initial animation in real time, check
 * Live Preview. Note that the live preview may run slower than your normal frame rate. The GameObject 
 * must have a SpriteRenderer attached.
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
            }
        }

        public void Start()
        {
            Play(sequenceName);
        }

        public void OnValidate()
        {
            UpdateCache();

            if (sequence == null || sequence.name != sequenceName)
                Play(sequenceName);
        }

        public void Play(string name)
        {            
            if (!cache.ContainsKey(name))
            {
                Debug.Log("Unknown sequence name: " + name);
                return;
            }

            sequence = cache[name];
            sequenceName = name;
            frame = 0;
            elapsed = 0;
            paused = false;
            UpdateView();
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

        public void Update()
        {
            if(paused)
                return;

            #if UNITY_EDITOR
            if(!Application.isPlaying && !livePreview)
                return;
            #endif

            if (sequence == null)
                return;

            elapsed += Time.deltaTime;
            if (elapsed >= sequence.deltaTime)
            {
                elapsed -= sequence.deltaTime;
                if (++frame >= sequence.frameArray.Count)
                    frame = 0;
                UpdateView();
            }
        }

        private void UpdateView()
        {
            if (sequence == null || sr == null)
                return;

            if (frame < 0 || frame >= sequence.frameArray.Count)
                return;

            int cel = sequence.frameArray[frame];
            sr.sprite = frames[cel];
        }

        public void UpdateCache()
        {
            // Recreate cache
            cache = new Dictionary<string, AnimSequence>();
            foreach (AnimSequence seq in sequences)
                cache.Add(seq.name, seq);

            // Preprocess sequence frames and fps
            foreach (AnimSequence seq in sequences)
            {
                seq.deltaTime = 1 / seq.fps;
                if (seq.deltaTime <= 0)
                    Debug.Log("Illegal fps:" + seq.fps);
                seq.frameArray = new List<int>();
                seq.frames = seq.frames.Replace(" ", "");
                foreach (string element in seq.frames.Split(','))
                {
                    // TODO Error reporting
                    if (element.Contains("-"))
                    {
                        string[] extents = element.Split('-');
                        int low = int.Parse(extents[0]);
                        int high = int.Parse(extents[1]);
                        for (int i = low; i <= high; i++)
                            seq.frameArray.Add(i);
                    }
                    else
                    {
                        int result = int.Parse(element);
                        seq.frameArray.Add(result);
                    }
                }
            }
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
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Anim))]
    public class AnimEditor : Editor
    {
        private SerializedProperty livePreview;
        private SerializedProperty sequenceName;
        private SerializedProperty frames;
        private SerializedProperty sequences;

        public void OnEnable()
        {
            livePreview = serializedObject.FindProperty("livePreview");
            sequenceName = serializedObject.FindProperty("sequenceName");
            frames = serializedObject.FindProperty("frames");
            sequences = serializedObject.FindProperty("sequences");
        }

        public override void OnInspectorGUI()
        {
            // Display inspector
            serializedObject.Update();
            EditorGUILayout.PropertyField(livePreview);
            EditorGUILayout.PropertyField(sequenceName);
            EditorGUILayout.PropertyField(frames, true);
            EditorGUILayout.PropertyField(sequences, true);
            serializedObject.ApplyModifiedProperties();

            // Support live preview
            Anim anim = (Anim)target;
            if (anim.livePreview)
                EditorUtility.SetDirty(target);
        }
    }

    [CustomPropertyDrawer(typeof(AnimSequence))]
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