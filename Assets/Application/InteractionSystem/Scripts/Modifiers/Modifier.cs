using UnityEngine;
using InteractionSystemToolsAndAssets;

/// <summary>
/// Provides static access to preset modifiers used by the <see cref="InteractionSystem"/>
/// </summary>
public static class Modifier
{
    #region Modifiers

    // Create accessors to modifiers here to be used in the project;

    // Default
    private static ActionMapModifier _default;
    public static ActionMapModifierPreset Default(Object owner) => Create(GetSource(_default, "InteractionSystem/Modifiers/Default"), owner);

    // Items
    private static ActionMapModifier _pickedUpItem_LeftHand;
    public static ActionMapModifierPreset PickedUpItem_LeftHand(Object owner) => Create(GetSource(_pickedUpItem_LeftHand, "InteractionSystem/Modifiers/Items/PickedUpItem - Left Hand"), owner);

    private static ActionMapModifier _pickedUpItem_RightHand;
    public static ActionMapModifierPreset PickedUpItem_RightHand(Object owner) => Create(GetSource(_pickedUpItem_RightHand, "InteractionSystem/Modifiers/Items/PickedUpItem - Right Hand"), owner);

    // Menus
    private static ActionMapModifier _menuInteraction_LeftHand;
    public static ActionMapModifierPreset MenuInteraction_LeftHand(Object owner) => Create(GetSource(_menuInteraction_LeftHand, "InteractionSystem/Modifiers/Menus/MenuInteraction - Left Hand"), owner);

    private static ActionMapModifier _menuInteraction_RightHand;
    public static ActionMapModifierPreset MenuInteraction_RightHand(Object owner) => Create(GetSource(_menuInteraction_RightHand, "InteractionSystem/Modifiers/Menus/MenuInteraction - Right Hand"), owner);

    //Misc
    private static ActionMapModifier _misc_DisablePlayer;
    public static ActionMapModifierPreset Misc_DisablePlayer(Object owner) => Create(GetSource(_misc_DisablePlayer, "InteractionSystem/Modifiers/Misc/DisablePlayer"), owner);

    #endregion Modifiers

    private static ActionMapModifierPreset Create(ActionMapModifier template, Object owner)
    {
        var copy = Object.Instantiate(template);

        copy.owner = owner;
        copy.name = copy.name.TrimEnd("(Clone)".ToCharArray());

        return new ActionMapModifierPreset(copy);
    }
    private static ActionMapModifier GetSource(ActionMapModifier source, string path)
    {
        if (source == null)
        {
            source = Resources.Load<ActionMapModifier>(path);

            if (source == null)
            {
                Debug.LogError($"Could not find {nameof(ActionMapModifier)} at path: {path}");
            }
        }

        return source;
    }
}