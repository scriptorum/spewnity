using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spewnity
{
    /// <summary>
    /// Property drawer for a Reposition attribute. See RepositionAttribute in Attributes.
    /// <para>This should be in the Editor folder.</para>
    /// </summary>
    [CustomPropertyDrawer(typeof (RepositionAttribute))]
    public class RepositionPD : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Show foldout title, but not contents, which we need to reorder
            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property, false);

            // If foldout opened, iterator through contents
            if (property.isExpanded)
            {
                // Look up all properties in this object
                List<string> propNames = new List<string>();
                SerializedProperty fieldProp = property.Copy();
                fieldProp.NextVisible(true);
                do {
                    SerializedProperty p = property.FindPropertyRelative(fieldProp.name);
                    if (p != null)
                        propNames.Add(fieldProp.name);
                }
                while (fieldProp.NextVisible(false));

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
                    else Debug.LogWarning("Property " + fieldName + " not found");
                }

                // Display the fields
                EditorGUI.indentLevel++;
                float y = position.y + EditorGUIUtility.singleLineHeight;
                foreach(string propName in propNames)
                {
                    SerializedProperty prop = property.FindPropertyRelative(propName);
                    float height = EditorGUI.GetPropertyHeight(prop, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), prop, true);
                    y += height;
                }
                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
                return EditorGUI.GetPropertyHeight(property, true) - EditorGUIUtility.singleLineHeight;
            return EditorGUIUtility.singleLineHeight;;
        }
    }
}