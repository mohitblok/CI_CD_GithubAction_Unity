using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using UnityEngine;

/// <summary>
/// Asset used by the <see cref="InteractionSystem"/>. Holds references to InputActionMaps to be enabled/disabled.
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "Modifier Name", menuName = "Bloktopia/Scriptable Objects/Interaction System/Action Map Modifier", order = 1)]
public class ActionMapModifier : ScriptableObject
{
    [HideInInspector] public Object owner;
    [field: SerializeField] public List<InputActionMapReference> MapsToEnable { get; private set; }
    [field: SerializeField] public List<InputActionMapReference> MapsToDisable { get; private set; }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Set name to name of file, excluding the extension
        name = Path.GetFileName(AssetDatabase.GetAssetPath(GetInstanceID())).Split(".asset")[0];
        MapsToEnable.ReplaceDuplicatesWithNull();
        MapsToDisable.ReplaceDuplicatesWithNull();
    }
#endif
}
