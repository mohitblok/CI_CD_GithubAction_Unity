using InteractionSystemToolsAndAssets;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InteractionSystemEditorTool
{
    /// <summary>
    /// Editor window providing a UI for the creation of InputActionMapReferences
    /// </summary>
    public class InputActionMapReferenceBuilderWindow : EditorWindow
    {
        private const string FULL_NAME = "InputActionMap Reference Builder";
        private static readonly string WINDOW_NAME = "IAM Reference Builder";
        private static readonly Vector2 MIN_WINDOW_SIZE = new Vector2(650, 150);
        private static readonly Vector2 MAX_WINDOW_SIZE = new Vector2(650, 750);

        private static bool _searched;
        public List<InputActionAsset> inputActionAssets = new();
        public List<InputActionAssetData> inputActionAssetDatas = new();

        [MenuItem("Tools/Interaction System/" + FULL_NAME)]
        private static void Init()
        {
            InputActionMapReferenceBuilderWindow window = (InputActionMapReferenceBuilderWindow)GetWindow(typeof(InputActionMapReferenceBuilderWindow));
            window.titleContent = new GUIContent(WINDOW_NAME);
            window.minSize = MIN_WINDOW_SIZE;
            window.maxSize = MAX_WINDOW_SIZE;

            _searched = false;
        }

        private void OnGUI()
        {
            if (!_searched)
            {
                GetAllInputActionAssets();
                _searched = true;
            }

            QuickGUIButton("Refresh Found Assets", GetAllInputActionAssets);
            GUILayout.Space(10);
            inputActionAssetDatas.ForEach(assetData => DrawIAADataEntry(assetData));
            GUILayout.Space(10);
            QuickGUIButton("Build Action Map References", BuildActionMapReferences);
        }

        private void BuildActionMapReferences()
        {
            inputActionAssetDatas.ForEach(assetData =>
            {
                if (!assetData.include)
                {
                    return;
                }

                InputActionMapReferenceBuilder.CreateAssets(assetData.asset, assetData.Path);
            });
        }

        private void GetAllInputActionAssets()
        {
            // Save to list
            inputActionAssets = AssetPaths.GetAllAssets();

            // Save references to class for drawing UI in editor window
            inputActionAssetDatas.Clear();
            inputActionAssets.ForEach(asset => inputActionAssetDatas.Add(new InputActionAssetData(asset)));
        }

        private void DrawIAADataEntry(InputActionAssetData entry)
        {
            EditorGUILayout.BeginHorizontal();

            entry.include = EditorGUILayout.Toggle(entry.include, GUILayout.Width(20));
            entry.asset = (InputActionAsset)EditorGUILayout.ObjectField(entry.asset, typeof(InputActionAsset), GUILayout.Width(250));
            entry.pathOutputType = (InputActionAssetData.PathOutput)EditorGUILayout.EnumPopup(entry.pathOutputType, GUILayout.Width(80));

            if (entry.pathOutputType == InputActionAssetData.PathOutput.Custom)
            {
                entry.userSpecifiedPath = EditorGUILayout.TextField(entry.userSpecifiedPath);
            }
            else
            {
                EditorGUILayout.LabelField("New assets will be created ajacent to this asset.");
            }

            EditorGUILayout.EndHorizontal();
        }

        private void QuickGUIButton(string text, Action action)
        {
            if (GUILayout.Button(text))
            {
                action.Invoke();
            }
        }
    }
}