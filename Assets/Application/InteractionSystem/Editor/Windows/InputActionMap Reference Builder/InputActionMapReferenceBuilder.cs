using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InteractionSystemEditorTool
{
    /// <summary>
    /// Utility class to create InputActionMapReferences
    /// </summary>
    public static class InputActionMapReferenceBuilder
    {
        private static readonly string FILE_TYPE_EXTENSION = ".asset";

        public static void CreateAssets(InputActionAsset inputActionAsset, string destinationPath)
        {
            inputActionAsset.actionMaps.ToList().ForEach(map =>
            {
                CreateAsset(inputActionAsset, map, destinationPath);
            });
        }

        private static void CreateAsset(InputActionAsset inputActionAsset, InputActionMap inputActionMap, string destinationPath)
        {
            string outputFileName = BuildFileName(inputActionAsset, inputActionMap);
            string outputFilePath = Path.Combine(destinationPath, outputFileName);

            if (File.Exists(outputFilePath))
            {
                var IAMR = AssetDatabase.LoadAssetAtPath<InputActionMapReference>(outputFilePath);
                IAMR.SetMap(inputActionAsset, inputActionMap);
                EditorUtility.SetDirty(IAMR);
                AssetDatabase.SaveAssets();
                return;
            }

            InputActionMapReference asset = ScriptableObject.CreateInstance<InputActionMapReference>();
            asset.SetMap(inputActionAsset, inputActionMap);

            Directory.CreateDirectory(destinationPath);
            AssetDatabase.CreateAsset(asset, Path.Combine(destinationPath, outputFileName));
        }

        private static string BuildFileName(InputActionAsset inputActionAsset, InputActionMap inputActionMap)
        {
            return $"{inputActionAsset.name} - {inputActionMap.name}{FILE_TYPE_EXTENSION}";
        }
    }
}
