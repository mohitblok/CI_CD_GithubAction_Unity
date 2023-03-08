using InteractionSystemToolsAndAssets;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Manages InputActionAssets by enabling and disabling maps based on modifiers
/// </summary>
public partial class InteractionSystem : MonoSingleton<InteractionSystem>
{
    private List<LinkedInputActionAssets> _linkedInputActionAssets;

    private List<ActionMapModifier> _modifiers = new();
    /// <summary>
    /// List of all currently active modifiers
    /// </summary>
    public static List<ActionMapModifier> modifiers => Instance._modifiers;

    /// <summary>
    /// Purely a callback for when the Interaction System is loaded.
    /// </summary>
    /// <param name="callback"></param>
    public void Init(Action callback)
    {
        callback?.Invoke();
    }

    private void OnEnable()
    {
        _linkedInputActionAssets ??= InitialiseAssets();
        EnableAllAssets();
        AddModifier(Modifier.Default(this));
    }

    private void EnableAllAssets()
    {
        _linkedInputActionAssets.ForEach(assets => assets.EnableAllMaps());
    }

    /// <summary>
    /// Returns a list of all <see cref="InputActionAsset"/>s managed by the <see cref="InteractionSystem"/>
    /// </summary>
    public static List<InputActionAsset> GetInputActionAssets()
    {
        if (Instance._linkedInputActionAssets == null)
        {
            return null;
        }

        List<InputActionAsset> output = new();
        Instance._linkedInputActionAssets.ForEach(assets => output.Add(assets.ResourceAsset));
        return output;
    }

    /// <summary>
    /// Adds a modifier preset and returns the activated modifier.
    /// <para>Save the returned modifier for later use to remove it.</para>
    /// </summary>
    /// <param name="preset">Use <see cref="Modifier"/> class to access known modifier presets</param>
    public static ActionMapModifier AddModifier(ActionMapModifierPreset preset)
    {
        modifiers.Add(preset.modifier);
        Instance.ApplyModifiers();

        return preset.modifier;
    }

    /// <summary>
    /// Removes an activated modifier.
    /// </summary>
    /// <param name="modifier">The modifier returned by <see cref="AddModifier(ActionMapModifierPreset)"/></param>
    public static void RemoveModifier(ActionMapModifier modifier)
    {
        if (modifier == null)
        {
            return;
        }

        modifiers.RemoveIfExists(modifier);
        modifier.MapsToEnable.ForEach(map => Instance._linkedInputActionAssets.ForEach(linkedAssets => linkedAssets.DisableMap(map.Map)));
        modifier.MapsToDisable.ForEach(map => Instance._linkedInputActionAssets.ForEach(linkedAssets => linkedAssets.EnableMap(map.Map)));
        Instance.ApplyModifiers();
    }

    private void ApplyModifiers()
    {
        List<InputActionMapReference> enableList = new();
        List<InputActionMapReference> disableList = new();

        modifiers.ForEach(modifier =>
        {
            enableList.RemoveRangeIfExists(modifier.MapsToDisable);
            disableList.RemoveRangeIfExists(modifier.MapsToEnable);

            enableList.AddRange(modifier.MapsToEnable);
            disableList.AddRange(modifier.MapsToDisable);
        });

        enableList.ForEach(inputActionMap => _linkedInputActionAssets.ForEach(linkedAssets => linkedAssets.EnableMap(inputActionMap.Map)));
        disableList.ForEach(inputActionMap => _linkedInputActionAssets.ForEach(linkedAssets => linkedAssets.DisableMap(inputActionMap.Map)));
    }
}