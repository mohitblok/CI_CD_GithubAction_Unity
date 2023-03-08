using UnityEngine;
using UnityEditor;

//TODO: DAVE: Refactor to Bloktopia Style Guide

namespace InfotainmentCore.Editor
{
	[CustomPropertyDrawer(typeof(InspectorNote))]
	class InspectorNotePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty NoteTitle = property.FindPropertyRelative("noteText");
			SerializedProperty NoteColour = property.FindPropertyRelative("noteColour");
			SerializedProperty useCustomColour = property.FindPropertyRelative("useCustomColour");

			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.normal.textColor =
				useCustomColour.boolValue ? NoteColour.colorValue : EditorStyles.label.normal.textColor;
			style.fontStyle = FontStyle.Bold;
			style.wordWrap = true;

			Color original = GUI.color;
			GUI.color = style.normal.textColor;
			GUI.Label(position, NoteTitle.stringValue, style);
			GUI.color = original;
		}
	}
}