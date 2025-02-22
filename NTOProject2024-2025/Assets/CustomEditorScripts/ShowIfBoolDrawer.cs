using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ShowIfBoolAttribute))]
public class ShowIfBoolDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfBoolAttribute showIf = (ShowIfBoolAttribute)attribute;
        SerializedProperty boolProperty = property.serializedObject.FindProperty(showIf.boolName);

        if (boolProperty != null && boolProperty.boolValue)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfBoolAttribute showIf = (ShowIfBoolAttribute)attribute;
        SerializedProperty boolProperty = property.serializedObject.FindProperty(showIf.boolName);

        return (boolProperty != null && boolProperty.boolValue) ? EditorGUI.GetPropertyHeight(property, label) : 0;
    }
}