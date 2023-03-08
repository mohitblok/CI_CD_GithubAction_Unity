using System;
using UnityEngine;

/// <summary>
/// This class extends the "BaseModule" class and provides module-specific implementation for scale-based modules.
/// </summary>
/// <see cref="BaseModule"/>
public class ScaleModule : BaseModule
{
    private new ScaleModuleData data;
    
    /// <summary>
    /// Creates a reference to the retrieved and apply module data
    /// It updates the position of the transform by adding the offset data to the current position.
    /// Finally, it invokes the callback function if it is not null.
    /// </summary>
    /// <param name="baseModuleData"></param>
    /// <param name="callback"></param>
    public override void Init(BaseModuleData baseModuleData, Action callback)
    {
        data = (ScaleModuleData) baseModuleData;

        gameObject.transform.localScale = data.scale;
        
        callback?.Invoke();
    }

    /// <summary>
    /// This overridden method is used to deinitialize a module and has no arguments.
    /// It is implemented in derived classes to provide module-specific cleanup logic.
    /// </summary>
    public override void Deinit() { }
}

/// <summary>
/// Data which is modifiable by users
/// </summary>
/// <see cref="BaseModuleData"/>
[Serializable]
public class ScaleModuleData : BaseModuleData
{
    /// <summary>
    /// The scale
    /// </summary>
    public Vector3 scale;
}
