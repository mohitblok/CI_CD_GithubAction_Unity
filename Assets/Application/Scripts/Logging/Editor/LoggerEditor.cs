using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Logging;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

public class LoggerEditor : EditorWindow
{
    private Dictionary<string, Toggle> allLoggers = new Dictionary<string, Toggle>();

    StringBuilder searchText = new StringBuilder();

    VisualElement root;


    [MenuItem("Window/LoggerEditor")]
    public static void ShowExample()
    {
        /*
        //ref : https://stackoverflow.com/questions/71322699/how-to-create-a-window-in-unity-from-a-script-and-attach-it-to-an-existing-tab
        var types = new List<Type>()
        { 
            // first add your preferences
            typeof(SceneView),
            typeof(Editor).Assembly.GetType("UnityEditor.GameView"),
            typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow"),
            typeof(Editor).Assembly.GetType("UnityEditor.ConsoleWindow"),
            typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser"),
            typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow")
        };

        // and then add all others as fallback (who cares about duplicates at this point ? ;) )
        types.AddRange(AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(EditorWindow))));        

        LoggerEditor wnd = GetWindow<LoggerEditor>(types.ToArray());
        wnd.titleContent = new GUIContent("LoggerEditor");
        */

        LoggerEditor wnd = GetWindow<LoggerEditor>();
        wnd.titleContent = new GUIContent("LoggerEditor");        
    }

    //to-do - refactor.
    public void OnInspectorUpdate()
    {    
        if (searchText.ToString().Length > 0)
        {
            Debug.Log("search string found");

            foreach (KeyValuePair<string, Toggle> kvp in allLoggers)
            {
                if (kvp.Key.Contains(searchText.ToString()))
                {
                    root.Remove(kvp.Value);
                    Repaint(); //repaint whole editor
                }
            }
        }
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UIToolkit/XML/LoggerWindow.uxml");
        //var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UIToolkit/Windows/GameLoggerV3.uxml");

        VisualElement loggerWindow = visualTree.Instantiate();

        root.Add(loggerWindow);

        Toggle debugToggle = new Toggle();
        debugToggle.name = "Debug";
        debugToggle.label = "Debug";
        debugToggle.value = true;

        root.Add(debugToggle);

        for (int i = 0; i < GameLog.AllLoggers.Count; i++)
        {
            //custom theme toggle
            //var customToggle = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UIToolkit/Templates/CustomToggle.uxml");
            //VisualElement toggleC = customToggle.Instantiate();
            //root.Add(toggleC);

            Toggle toggle = new Toggle();

            var name = GameLog.AllLoggers[i].Key;
            toggle.name = name;
            toggle.label = name;
            toggle.value = true;

            allLoggers.Add(name, toggle);

            root.Add(toggle);
        }

        //Call the event handler
        SetupEventsHandler();

    }

    //Functions as the event handlers for your button click and number counts 
    private void SetupEventsHandler()
    {
        VisualElement root = rootVisualElement;

        var toggles = root.Query<Toggle>();
        toggles.ForEach(RegisterHandler);


        var search = root.Query<ToolbarPopupSearchField>();
        search.ForEach(RegisterSearchHandle);
    }

    private void RegisterSearchHandle(ToolbarPopupSearchField searchField)
    {
        searchField.RegisterCallback<KeyUpEvent>(PrintSearchMessage);
    }

    private void PrintSearchMessage(KeyUpEvent evt)
    {
        ToolbarPopupSearchField t = evt.currentTarget as ToolbarPopupSearchField;

        //Debug.Log(evt.keyCode.ToString());
        if (evt.keyCode == KeyCode.Backspace || evt.keyCode == KeyCode.Delete)
        {
            t.Clear();
            searchText.Clear();
        }        

        if(evt.keyCode.ToString().Length == 1 && char.IsLetter(evt.keyCode.ToString()[0]))
        {
            searchText.Append(evt.keyCode.ToString());
        }

        if (evt.keyCode == KeyCode.Return)
        {

        }
        Debug.Log(searchText.ToString());
    }

    private void RegisterHandler(Toggle toggle)
    {
        toggle.RegisterCallback<ClickEvent>(PrintClickMessage);
    }

    private void PrintClickMessage(ClickEvent evt)
    {
        VisualElement root = rootVisualElement;

        Toggle toggle = evt.currentTarget as Toggle;

        if (toggle.value)
        {
            for (int i = 0; i < GameLog.AllLoggers.Count; i++)
            {
                string name = GameLog.AllLoggers[i].Key;

                if(toggle.name == name)
                {
                    GameLog.AllLoggers[i].Value.Enable();
                }
            }

            if(toggle.name == "Debug")
            {
                GameLog.EnterDebugMode();
                Debug.Log("Debug mode enter");
                ControlToggles(true);
            }
        }
        else
        {
            for (int i = 0; i < GameLog.AllLoggers.Count; i++)
            {
                string name = GameLog.AllLoggers[i].Key;

                if (toggle.name == name)
                {
                    GameLog.AllLoggers[i].Value.Disable();
                }

            }

            if (toggle.name == "Debug")
            {
                GameLog.ExitDebugMode();
                Debug.Log("Debug mode exited");
                ControlToggles(false);
            }
        }
    }

    private void ControlToggles(bool b)
    {
        foreach (KeyValuePair<string, Toggle> kvp in allLoggers)
        {
            if (b)
            {
                kvp.Value.value = true;
            }
            else
            {
                kvp.Value.value = false;
            }
        }
    }
}