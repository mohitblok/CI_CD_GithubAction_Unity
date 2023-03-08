using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InteractionSystemToolsAndAssets
{
    /// <summary>
    /// Hold references to the InputActionAssets for the InteractionSystem
    /// </summary>
    public static class AssetPaths
    {
        private static readonly string BASE_PATH = "InteractionSystem/InputActionAssets/";

        /// <summary>
        /// Path to PCGamepad InputActionAsset
        /// </summary>
        public static readonly string PCGamepad = $"{BASE_PATH}PCGamepad/PCGamepad";
        /// <summary>
        /// Path to VirtualReality InputActionAsset
        /// </summary>
        public static readonly string VirtualReality = $"{BASE_PATH}VirtualReality/VirtualReality";
        /// <summary>
        /// Path to OpenXR InputActionAsset
        /// </summary>
        public static readonly string OpenXR = $"{BASE_PATH}OpenXRInput/OpenXRInput";
        /// <summary>
        /// Path to Debugging InputActionAsset
        /// </summary>
        public static readonly string Debugging = $"{BASE_PATH}DebugInput/DebugInput";

        private static List<string> AllAssetPaths => new List<string>()
        { PCGamepad, VirtualReality, OpenXR, Debugging };

        /// <summary>
        /// Loads all speified <see cref="InputActionAsset"/>s defined in <see cref="AllAssetPaths"/>
        /// </summary>
        /// <returns></returns>
        public static List<InputActionAsset> GetAllAssets()
        {
            var output = new List<InputActionAsset>();
            AllAssetPaths.ForEach(assetPath => output.Add(Resources.Load<InputActionAsset>(assetPath)));
            return output;
        }
    }
}
