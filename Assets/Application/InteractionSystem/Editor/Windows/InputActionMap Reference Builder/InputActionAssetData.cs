using UnityEditor;
using UnityEngine.InputSystem;

namespace InteractionSystemEditorTool
{
    /// <summary>
    /// Data type used by editor window <see cref="InputActionMapReferenceBuilderWindow"/> to define where and how <see cref="InputActionMapReference"/>s are created.
    /// </summary>
    [System.Serializable]
    public class InputActionAssetData
    {
        private static readonly string ADJACENT_FOLDER_NAME = "\\Action Map References";
        public enum PathOutput
        {
            Adjacent, Custom
        }
        public InputActionAsset asset;
        public bool include = true;
        public PathOutput pathOutputType;
        public string userSpecifiedPath;
        public string Path
        {
            get
            {
                switch (pathOutputType)
                {
                    case PathOutput.Adjacent: return GetDirectoryOfAsset(asset) + ADJACENT_FOLDER_NAME;
                    case PathOutput.Custom: return userSpecifiedPath;
                    default: return null;
                }
            }
        }

        public InputActionAssetData(InputActionAsset asset)
        {
            this.asset = asset;
        }

        private static string GetDirectoryOfAsset(InputActionAsset asset)
        {
            return System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(asset.GetInstanceID()));
        }
    }
}
