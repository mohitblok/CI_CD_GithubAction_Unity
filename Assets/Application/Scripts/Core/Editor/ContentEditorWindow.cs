using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// The ContentEditorWindow class is the main window for the content editor.
/// </summary>
public class ContentEditorWindow : BaseEditorWindow
{
    private static ContentEditorWindow window;

    /// <summary> The ShowContentEditorWindow function creates a window that allows the user to view and edit content data.</summary>
    [MenuItem("Tools/Editor Windows/Content Data", false, 1)]
    public static void ShowContentEditorWindow()
    {
        window = GetWindow<ContentEditorWindow>(false, "Content Data", true);
        window.Show();
    }

    private TopBarEditorWindow topBarEditorWindow;
    private static BaseEditorWindow currentScreen;

    private protected override void OnEnable()
    {
        base.OnEnable();
        if (!isInitialised)
            SetupContentManager();
        
        topBarEditorWindow = CreateInstance<TopBarEditorWindow>();

        topBarEditorWindow.onContentTabSelected += ShowContent<ContentDataEditorWindow>;
        topBarEditorWindow.onRefreshSelected += () => { currentScreen.Refresh(); };

        topBarEditorWindow.onContentTabSelected.Invoke();
        Repaint();
    }

    protected internal override void Refresh()
    {
        base.Refresh();
    }

    private static void ShowContent<T>() where T : BaseEditorWindow
    {
        if (currentScreen)
        {
            currentScreen.DeInit();
        }

        currentScreen = CreateInstance<T>();
        currentScreen.Init();
    }

    private bool isInitialised = false;
    
    private void OnGUI()
    {
        if (!isInitialised)
        {
            return;
        }
        
        if (!topBarEditorWindow) return;

        topBarEditorWindow.ManualOnGUI();

        if (!currentScreen) return;

        currentScreen.ManualOnGUI();
    }

    private void SetupContentManager()
    {
        MainThreadDispatcher.CreateInstance();
        CoroutineUtil.CreateInstance();
        DomainManager.CreateInstance();
        DomainManager.Instance.Init(() =>
        {
            Debug.Log("Domain manager init complete");
            WebRequestManager.CreateInstance();
            ContentManager.CreateInstance();
            WebRequestManager.Instance.WakeUp();
            isInitialised = true;
        });
    }
}

/// <summary>
/// The TopBarEditorWindow class is the top bar for the content editor.
/// </summary>
public class TopBarEditorWindow : BaseEditorWindow
{
    private const string CONTENT_TAB_NAME = "Content";
    private const string REFRESH = "Refresh";

    protected internal Action onContentTabSelected;
    protected internal Action onRefreshSelected;

    protected internal override void ManualOnGUI()
    {
        EditorGUILayout.BeginHorizontal(GUI.skin.box);

        DrawTabButton(CONTENT_TAB_NAME, EditorConsts.SHORT_BUTTON_WIDTH, () => { onContentTabSelected?.Invoke(); });

        DrawTabButton("Load All", EditorConsts.SHORT_BUTTON_WIDTH, LoadAll);

        GUILayout.FlexibleSpace();

        DrawTabButton(REFRESH, 80, () => { onRefreshSelected?.Invoke(); });

        EditorGUILayout.EndHorizontal();
    }

    private void DrawTabButton(string label, int width, Action onClick)
    {
        if (GUILayout.Button(label, GUILayout.Width(width), GUILayout.Height(EditorConsts.THICK_BUTTON_HEIGHT)))
        {
            onClick?.Invoke();
        }
    }

    private void LoadAll()
    {
       ContentManager.Instance.GetData(entry =>
       {
           
       });
    }
}