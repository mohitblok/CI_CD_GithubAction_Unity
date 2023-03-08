namespace InteractionSystemToolsAndAssets
{
    /// <summary>
    /// A container for preventation of passing custom modifiers created at runtime into the InteractionSystem
    /// </summary>
    public class ActionMapModifierPreset
    {
        public ActionMapModifier modifier { get; private set; }

        public ActionMapModifierPreset(ActionMapModifier modifier)
        {
            this.modifier = modifier;
        }
    }
}