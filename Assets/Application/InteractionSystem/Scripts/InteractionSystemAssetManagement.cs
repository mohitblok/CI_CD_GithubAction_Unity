using InputActionAssetCode;
using InteractionSystemToolsAndAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Loads the InputActionAssets required by the project.
// Loads production or development assets depending on build conditions.
// InputActionAssets can be safely added and removed without interfering with the wider InteractionSystem behaviour.

public partial class InteractionSystem : MonoSingleton<InteractionSystem>
{
    #region InputActionAssets

    /// Configure the InputActionAssets and their generated C# files here.
    /// Don't forget to update the <see cref="InitialiseProductionAssets"/> and <see cref="InitialiseDevelopmentAssets"/> methods

    private PCGamepad _pcGamepadCode;
    public static PCGamepad PCGamepad => Instance._pcGamepadCode;

    private VirtualReality _virtualRealityCode;
    public static VirtualReality VirtualReality => Instance._virtualRealityCode;

    private OpenXRInput _openXRInputCode;
    public static OpenXRInput OpenXRInput => Instance._openXRInputCode;

    private DebugInput _debugInputCode;
    public static DebugInput DebugInput => Instance._debugInputCode;

    #endregion InputActionAssets

    private List<LinkedInputActionAssets> InitialiseAssets()
    {
// #if UNITY_EDITOR
//         return InitialiseDevelopmentAssets();
// #else
//          return Debug.isDebugBuild ? InitialiseDevelopmentAssets : InitialiseProductionAssets;
// #endif
    }
    private List<LinkedInputActionAssets> InitialiseProductionAssets()
    {
        List<LinkedInputActionAssets> output = new();

        _pcGamepadCode = new PCGamepad();
        output.Add(LinkAssets(_pcGamepadCode.asset, AssetPaths.PCGamepad));

        _virtualRealityCode = new VirtualReality();
        output.Add(LinkAssets(_virtualRealityCode.asset, AssetPaths.VirtualReality));

        _openXRInputCode = new OpenXRInput();
        output.Add(LinkAssets(_openXRInputCode.asset, AssetPaths.OpenXR));

        return output;
    }
    private List<LinkedInputActionAssets> InitialiseDevelopmentAssets()
    {
        List<LinkedInputActionAssets> output = new(InitialiseProductionAssets());

        _debugInputCode = new DebugInput();
        output.Add(LinkAssets(_debugInputCode.asset, AssetPaths.Debugging));

        return output;
    }

    private LinkedInputActionAssets LinkAssets(InputActionAsset codeAsset, string resourceAssetPath)
    {
        return new LinkedInputActionAssets(codeAsset, Resources.Load<InputActionAsset>(resourceAssetPath));
    }
}
