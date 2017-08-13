using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

// TODO New rangeTypes ObjectRelative and SourceRelative
// TODO Cache TweenTemplate names in dictionary?
// TODO Add ability to spawn Tween dynamically using template parameters
// TODO And also spawn template
// TODO And also manipulate template list
// TODO Ability to modify template parameters in tween
// TODO Add Reverse, PingPong, and Cycles/Looping, although some of this can be approximated with easing
// TODO A Vec4 lerp is only needed for color, other TweenTypes will be more performant if you lerp just the parts you need.
// TODO Add ColorWithoutAlpha?
namespace Spewnity
{
    public class TweenManager : MonoBehaviour
    {
        public static TweenManager instance;
        public List<TweenTemplate> tweenTemplates;
        private List<Tween> tweens; // active, running tweens

        // Creates a new tween from the template and runs it
        public void Play(string tweenTemplateName)
        {
            Play(GetTween(tweenTemplateName));
        }

        // Runs the tween, see Create
        public void Play(Tween tween)
        {
            tween.value = tween.startValue;
            tween.timeRemaining = tween.template.duration;
            tweens.Add(tween);
            tween.template.events.Start.Invoke(tween);
        }

        // Creates a new Tween using the tween template
        // You can modify the start and end values as needed
        public Tween GetTween(string tweenTemplateName)
        {
            return GetTween(GetTemplate(tweenTemplateName));
        }

        // Creates a new Tween using the tween template
        // You can modify the start and end values as needed
        public Tween GetTween(TweenTemplate template)
        {
            Tween t = new Tween(template);
            return t;
        }

        // Returns the template
        // It's not recommended to modify the template, unless you always set the same parameters before creating tweens
        public TweenTemplate GetTemplate(string tweenTemplateName)
        {
            TweenTemplate template = tweenTemplates.Find(x => x.name == tweenTemplateName);
            if (template == null)
                throw new UnityException("TweenTemplate not found:" + tweenTemplateName);
            return template;
        }

        void Awake()
        {
            if (instance != this)
                instance = this;
            else Debug.Log("There are multiple TweenManagers. Instance is set to the first.");
            tweens = new List<Tween>();
        }
        void Update()
        {
            Color color;

            foreach(Tween tween in tweens)
            {
                TweenTemplate template = tween.template;
                tween.timeRemaining -= Time.deltaTime;
                if (tween.timeRemaining < 0f)
                    tween.timeRemaining = 0f;
                float t = 1 - tween.timeRemaining / template.duration;
                if (template.easing.length > 0)
                    t = template.easing.Evaluate(t);
                Vector4 value = Vector4.LerpUnclamped(tween.startValue.value, tween.endValue.value, t);
                tween.value.value = value;

                // Apply tween to object
                switch (template.tweenType)
                {
                    case TweenType.Float:
                        break; // nothing to do here, no object, just callback

                    case TweenType.SpriteRendererAlpha:
                        color = template.spriteRenderer.color;
                        color.a = value.x;
                        template.spriteRenderer.color = color;
                        break;

                    case TweenType.TextAlpha:
                        color = template.text.color;
                        color.a = value.x;
                        template.text.color = color;
                        break;

                    case TweenType.SpriteRendererColor:
                        template.spriteRenderer.color = (Color) value;
                        break;

                    case TweenType.TextColor:
                        template.text.color = (Color) value;
                        break;

                    case TweenType.LocalPosition2D:
                        template.transform.localPosition = new Vector3(value.x, value.y, template.transform.localPosition.z);
                        break;

                    case TweenType.LocalPosition3D:
                        template.transform.localPosition = (Vector3) value;
                        break;

                    case TweenType.Position2D:
                        template.transform.position = new Vector3(value.x, value.y, template.transform.position.z);
                        break;

                    case TweenType.Position3D:
                        template.transform.position = (Vector3) value;
                        break;

                    case TweenType.Rotation2D:
                        Vector3 vec = template.transform.eulerAngles;
                        vec.z = value.x;
                        template.transform.eulerAngles = vec;
                        break;

                    case TweenType.Rotation3D:
                        template.transform.eulerAngles = (Vector3) value;
                        break;

                    case TweenType.Scale2D:
                        template.transform.localScale = new Vector3(value.x, value.y, template.transform.localScale.z);
                        break;

                    case TweenType.Scale3D:
                        template.transform.localScale = (Vector3) value;
                        break;

                    default:
                        Debug.Log("Unknown TweenType:" + template.tweenType);
                        break;

                }

                template.events.Change.Invoke(tween);
                if (tween.timeRemaining <= 0f)
                    template.events.End.Invoke(tween);
            }

            tweens.RemoveAll(t => t.timeRemaining <= 0);
        }
    }

    [System.Serializable]
    public class TweenTemplate
    {
        public string name;
        public float duration;
        public TweenType tweenType;
        public RangeType rangeType;
        public TweenValue source;
        public Transform transform = null;
        public SpriteRenderer spriteRenderer = null;
        public Text text = null;
        public TweenValue dest;
        public AnimationCurve easing;
        public TweenEvents events;

        public TweenValue GetObjectValue()
        {
            switch (tweenType)
            {
                case TweenType.SpriteRendererAlpha:
                case TweenType.SpriteRendererColor:
                    spriteRenderer.ThrowIfNull();
                    return new TweenValue((Vector4) spriteRenderer.color);

                case TweenType.TextAlpha:
                case TweenType.TextColor:
                    text.ThrowIfNull();
                    return new TweenValue((Vector4) text.color);

                case TweenType.LocalPosition2D:
                case TweenType.LocalPosition3D:
                    transform.ThrowIfNull();
                    return new TweenValue((Vector4) transform.localPosition);

                case TweenType.Position2D:
                case TweenType.Position3D:
                    transform.ThrowIfNull();
                    return new TweenValue((Vector4) transform.position);

                case TweenType.LocalRotation2D:
                case TweenType.LocalRotation3D:
                    transform.ThrowIfNull();
                    return new TweenValue((Vector4) transform.localEulerAngles);

                case TweenType.Rotation2D:
                case TweenType.Rotation3D:
                    transform.ThrowIfNull();
                    return new TweenValue((Vector4) transform.eulerAngles);

                case TweenType.Scale2D:
                case TweenType.Scale3D:
                    transform.ThrowIfNull();
                    return new TweenValue((Vector4) transform.localScale);

                default:
                    throw new UnityException("TweenType" + tweenType + " does not have an object value");
            }
        }
    }

    [System.Serializable]
    public struct TweenEvents
    {
        public TweenEvent Start;
        public TweenEvent Change;
        public TweenEvent End;
    }

    public class Tween
    {
        public TweenValue value;
        public TweenValue startValue;
        public TweenValue endValue;
        public float timeRemaining;
        public TweenTemplate template;

        public Tween(TweenTemplate template)
        {
            switch (template.rangeType)
            {
                case RangeType.ObjectToDest:
                    startValue = template.GetObjectValue();
                    endValue = template.dest;
                    break;

                case RangeType.SourceToDest:
                    startValue = template.source;
                    endValue = template.dest;
                    break;

                case RangeType.SourceToObject:
                    startValue = template.source;
                    endValue = template.GetObjectValue();
                    break;
            }

            this.template = template;
            value = startValue;
            timeRemaining = 0f;
        }
    }

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

        public float GetFloat()
        {
            return value.x;
        }

        public Vector2 GetVector2()
        {
            return (Vector2) value;
        }

        public Vector3 GetVector3()
        {
            return (Vector3) value;
        }

        public Color GetColor()
        {
            return (Color) value;
        }

        public void SetFloat(float f)
        {
            value = new Vector4(f, 0, 0, 0);
        }

        public void SetVector2(Vector2 v)
        {
            value = (Vector4) v;
        }

        public void SetVector3(Vector3 v)
        {
            value = (Vector4) v;
        }

        public void SetVector4(Vector4 v)
        {
            value = v;
        }
    }

    [System.Serializable]
    public class TweenEvent : UnityEvent<Tween> { }

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
        Float
    }

    [System.Serializable]
    public enum RangeType
    {
        ObjectToDest,
        SourceToObject,
        SourceToDest
    }

    //////////////////////////////////////////////////////////////////////////////// 

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof (TweenTemplate))]
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

    [CustomPropertyDrawer(typeof (TweenValue))]
    public class TweenValuePropertyDrawer : PropertyDrawer
    {
        private SerializedProperty rangeTypeProp;
        private SerializedProperty tweenTypeProp;
        private float lastX;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if (IsHidden(prop)) return;

            TweenType tweenType = (TweenType) tweenTypeProp.enumValueIndex;
            EditorGUI.BeginProperty(pos, label, prop);
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            SerializedProperty value = prop.FindPropertyRelative("value");
            lastX = 30;

            AddLabel(value, pos, prop.displayName, 90f);
            switch (tweenType)
            {
                case TweenType.Float:
                AddField(value, pos, "x", 100f, 35f, "value");
                break;

                case TweenType.SpriteRendererColor:
                case TweenType.TextColor:
                AddField(value, pos, "x", 46f, 10f, "r");
                AddField(value, pos, "y", 46f, 10f, "g");
                AddField(value, pos, "z", 46f, 10f, "b");
                AddField(value, pos, "w", 46f, 10f, "a");
                break;

                case TweenType.SpriteRendererAlpha:
                case TweenType.TextAlpha:
                AddField(value, pos, "x", 100f, 35f, "alpha");
                break;

                case TweenType.LocalPosition3D:
                case TweenType.Position3D:
                case TweenType.LocalRotation3D:
                case TweenType.Rotation3D:
                case TweenType.Scale3D:
                AddField(value, pos, "x", 65f, 10f);
                AddField(value, pos, "y", 65f, 10f);
                AddField(value, pos, "z", 65f, 10f);
                break;

                case TweenType.LocalPosition2D:
                case TweenType.Position2D:
                case TweenType.Scale2D:
                AddField(value, pos, "x", 90f, 10f);
                AddField(value, pos, "y", 90f, 10f);
                break;

                case TweenType.LocalRotation2D:
                case TweenType.Rotation2D:
                AddField(value, pos, "x", 100f, 35f, "angle");
                break;

                default:
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
            if (rangeTypeProp == null)
            {
                string tweenPath = prop.propertyPath.Substring(0, prop.propertyPath.LastIndexOf("."));
                string rangeTypePath = tweenPath + ".rangeType";
                string tweenTypePath = tweenPath + ".tweenType";
                rangeTypeProp = prop.serializedObject.FindProperty(rangeTypePath);
                tweenTypeProp = prop.serializedObject.FindProperty(tweenTypePath);
            }

            RangeType rangeType = (RangeType) rangeTypeProp.enumValueIndex;
            return (rangeType == RangeType.SourceToObject && prop.name == "dest") ||
                (rangeType == RangeType.ObjectToDest && prop.name == "source");
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            if (IsHidden(prop)) return -EditorGUIUtility.standardVerticalSpacing;
            return EditorGUI.GetPropertyHeight(prop, label, false);
        }
    }

    [CustomPropertyDrawer(typeof (Transform))]
    [CustomPropertyDrawer(typeof (SpriteRenderer))]
    [CustomPropertyDrawer(typeof (Text))]
    public class ObjectPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty tweenTypeProp;
        private SerializedProperty rangeTypeProp;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if (IsHidden(prop)) return;
            EditorGUI.PropertyField(pos, prop, new GUIContent("Object"));
        }

        private bool IsHidden(SerializedProperty prop)
        {
            if (rangeTypeProp == null)
            {
                string tweenPath = prop.propertyPath.Substring(0, prop.propertyPath.LastIndexOf("."));
                string rangeTypePath = tweenPath + ".rangeType";
                string tweenTypePath = tweenPath + ".tweenType";
                rangeTypeProp = prop.serializedObject.FindProperty(rangeTypePath);
                tweenTypeProp = prop.serializedObject.FindProperty(tweenTypePath);
            }

            TweenType tweenType = (TweenType) tweenTypeProp.enumValueIndex;
            RangeType rangeType = (RangeType) rangeTypeProp.enumValueIndex;
            switch (tweenType)
            {
                case TweenType.Float:
                rangeTypeProp.enumValueIndex = (int) RangeType.SourceToDest; // force this on float 
                return true; // float always hides objects

                case TweenType.SpriteRendererColor:
                case TweenType.SpriteRendererAlpha:
                return prop.name != "spriteRenderer";

                case TweenType.TextColor:
                case TweenType.TextAlpha:
                return prop.name != "text";
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