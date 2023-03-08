using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(StringListDropdown))]
[CanEditMultipleObjects]
public class StringListDropdownDrawer : PropertyDrawer
{
    public SerializedProperty strings;
    public SerializedProperty selectedIndex;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Cache properties
        strings = property.FindPropertyRelative("strings");
        selectedIndex = property.FindPropertyRelative("selectedIndex");

        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Get list of dropdown options
        List<string> possibleNames = new List<string>();
        for (int i = 0; i < strings.arraySize; i++)
        {
            possibleNames.Add(strings.GetArrayElementAtIndex(i).stringValue);
        }

        // Allow for selecting multiple objects in the scene without modifying properties
        EditorGUI.BeginChangeCheck();

        // Display list of dropdown options
        int newSelection = EditorGUI.Popup(position, selectedIndex.intValue, possibleNames.ToArray());

        // If value has changed, change the selected index, works on multiple selected objects
        if (EditorGUI.EndChangeCheck())
            selectedIndex.intValue = newSelection;

        EditorGUI.EndProperty();
    }
}
