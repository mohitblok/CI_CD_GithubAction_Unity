using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Holds a reference to a Map in an InputActionAsset. Used by <see cref="ActionMapModifier"/>s in the <see cref="InteractionSystem"/>.
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "Action Map Name", menuName = "Bloktopia/Scriptable Objects/Interaction System/Action Map Reference", order = 1)]
public class InputActionMapReference : ScriptableObject
{
    /// <summary>
    /// The target InputActionMap
    /// </summary>
    public InputActionMap Map => inputActionAsset.FindActionMap(targetActionMap.selectedString);

    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private StringListDropdown targetActionMap = new();

#if UNITY_EDITOR
    private readonly string NO_ASSET_DEFINED = "No Asset Defined";
    private static readonly string FILE_TYPE_EXTENSION = ".asset";

    private void OnValidate()
    {
        GenerateMapNameOptions(inputActionAsset);
        if (Selection.objects.ToList().Contains(this))
        {
            EditorApplication.delayCall += RenameAsset;
        }
    }

    private void GenerateMapNameOptions(InputActionAsset inputActionAsset)
    {
        if (inputActionAsset == null)
        {
            targetActionMap.strings = new string[] { NO_ASSET_DEFINED };
            return;
        }

        List<string> actionMapNames = new();
        inputActionAsset.actionMaps.ToList().ForEach(map => actionMapNames.Add(map.name));
        targetActionMap.strings = actionMapNames.ToArray();
    }

    private void SelectMapNameOption(InputActionMap inputActionMap)
    {
        int index = targetActionMap.GetIndexOfString(inputActionMap.name);

        if (index == -1)
        {
            Debug.LogError($"Could not find InputActionMap with name{inputActionMap.name}");
            return;
        }

        targetActionMap.selectedIndex = index;
    }

    private void RenameAsset()
    {
        if (inputActionAsset == null)
        {
            return;
        }

        string thisAssetPath = AssetDatabase.GetAssetPath(GetInstanceID());
        string currentName = Path.GetFileName(thisAssetPath);
        string newName = BuildFileName();
        string newAssetPath = Path.Combine(Path.GetDirectoryName(thisAssetPath), newName);

        if (currentName == newName)
        {
            return;
        }

        CheckForExistingAsset(newAssetPath, ref newName);

        AssetDatabase.RenameAsset(thisAssetPath, newName);
        AssetDatabase.SaveAssets();
    }

    private void CheckForExistingAsset(string newAssetPath, ref string newName)
    {
        if (File.Exists(newAssetPath))
        {
            var assetAtIntendedPath = AssetDatabase.LoadAssetAtPath<InputActionMapReference>(newAssetPath);
            int assetAtIntendedPathID = assetAtIntendedPath.GetInstanceID();

            // Check if there is a different file with the name this one intends to use
            if (assetAtIntendedPathID != GetInstanceID())
            {
                newName = newName.Split(".asset")[0] + " [Duplicate]";
                Debug.LogError("InputActionMapReference already exists", assetAtIntendedPath);
            }
        }
    }

    private string BuildFileName()
    {
        return $"{inputActionAsset.name} - {targetActionMap.selectedString}{FILE_TYPE_EXTENSION}";
    }

    /// <summary>
    /// Called by the editor tool to set the properties of this ScriptableObject
    /// </summary>
    /// <param name="inputActionAsset">The Input Action Asset to target the Action Map from</param>
    /// <param name="inputActionMap">The Action Map</param>
    public void SetMap(InputActionAsset inputActionAsset, InputActionMap inputActionMap)
    {
        this.inputActionAsset = inputActionAsset;

        GenerateMapNameOptions(inputActionAsset);
        SelectMapNameOption(inputActionMap);
    }
#endif
}
