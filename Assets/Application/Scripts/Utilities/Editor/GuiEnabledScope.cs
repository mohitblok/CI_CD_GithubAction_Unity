using UnityEngine;

public class GuiEnabledScope : GUI.Scope
{
    private static bool ParentEnabled = true;

    private readonly bool m_WasEnabled;

    public GuiEnabledScope(bool enabled)
    {
        m_WasEnabled = GUI.enabled;
        GUI.enabled = ParentEnabled && enabled;
    }

    protected override void CloseScope()
    {
        GUI.enabled = m_WasEnabled;
        ParentEnabled = m_WasEnabled;
    }
}