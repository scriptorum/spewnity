using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spewnity
{
    /// <summary>
    /// In the inspector, repositions the named properties to the top of the list.
    /// For example, [Reposition("name","age")] makes sure that the name field is 
    /// at the top, followed by age, and then the rest of the properties.
    /// </summary>
    public class RepositionAttribute : PropertyAttribute
    {
        public string[] fields;

        public RepositionAttribute(params string[] fields)
        {
            this.fields = fields;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Property drawer for a Reposition attribute. See RepositionAttribute.
    /// </summary>
    [CustomPropertyDrawer(typeof (RepositionAttribute))]
    public class RepositionPD : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty prop;

            // Look up all properties in this object
            List<string> propNames = new List<string>();
            prop = property.Copy();
            prop.NextVisible(true);
            do {
                SerializedProperty tempProp = property.FindPropertyRelative(prop.name);
                if (tempProp != null)
                    propNames.Add(tempProp.name);
            }
            while (prop.NextVisible(false));

            // Reposition specific fields to front
            RepositionAttribute repo = attribute as RepositionAttribute;
            for (int i = repo.fields.Length - 1; i >= 0; i--)
            {
                string fieldName = repo.fields[i];
                int propIdx = propNames.FindIndex((t) => t == fieldName);
                if (propIdx >= 0)
                {
                    propNames.RemoveAt(propIdx);
                    propNames.Insert(0, fieldName);
                }
                else Debug.LogWarning("Property '" + fieldName + "' not found");
            }

            // If first property is a non-empty string, it changes the name of the foldout title
            prop = property.FindPropertyRelative(propNames[0]);
            GUIContent foldoutLabel = new GUIContent(prop.type == "string" && !prop.stringValue.IsEmpty() ?
                prop.stringValue : "Element " + RepositionPD.GetIndexFromPropertyPath(property.propertyPath));

            // Show foldout title, but not contents, which we need to reorder
            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                property, foldoutLabel, false);

            // If foldout opened, iterator through contents
            if (property.isExpanded)
            {
                // Display the fields
                EditorGUI.indentLevel++;
                float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                foreach(string propName in propNames)
                {
                    prop = property.FindPropertyRelative(propName);
                    float height = EditorGUI.GetPropertyHeight(prop, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), prop, true);
                    y += height + EditorGUIUtility.standardVerticalSpacing;
                }
                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }

        public static string GetIndexFromPropertyPath(string path)
        {
            int start = path.LastIndexOf("[") + 1;
            int length = path.LastIndexOf("]") - start;
            return path.Substring(start, length);
        }
    }
#endif
}