using UnityEditor;

/// <summary>
/// Base class for all editor windows
/// </summary>
public class BaseEditorWindow : EditorWindow
{
    private protected virtual void OnEnable()
    {
        Repaint();
    }
        
    protected internal virtual void ManualOnGUI()
    {
    }

    protected internal virtual void Refresh()
    {
    }

    protected internal virtual void Init()
    {
    }

    protected internal virtual void DeInit()
    {
    }
}