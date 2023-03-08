using UnityEngine;
using UnityEditor;

namespace InfotainmentCore.Editor
{
	[CustomPropertyDrawer(typeof(InspectorButton))]
	class InspectorButtonDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty buttonTitle = property.FindPropertyRelative("buttonText");
			SerializedProperty buttonColour = property.FindPropertyRelative("buttonColour");

			GUILayout.ExpandWidth(true);

			Color originalBackgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = buttonColour.colorValue;

			if (GUI.Button(position, buttonTitle.stringValue))
			{
				System.Object buttonTarget = property.serializedObject.targetObject;

				if (buttonTarget.GetType().GetField(property.name) == null)
				{
					Debug.Log(
						"Could not find function call.. Is your button on a class inside a Monobehaviour.. Dont do this for now.");
				}
				else
				{
					buttonTarget.GetType().GetField(property.name).GetValue(buttonTarget).GetType()
						.GetMethod("ButtonPress")
						.Invoke(buttonTarget.GetType().GetField(property.name).GetValue(buttonTarget), null);
				}
			}

			GUI.backgroundColor = originalBackgroundColor;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) + 5; // Add # pixels height to this property drawer
		}
	}
}