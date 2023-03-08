using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logging {

    public class Log
    {
        public bool isSelected;
        public string info;
        public string message;
        public LogType type;

        public Log(bool isSelected, string info, string message, LogType type)
        {
            this.isSelected = isSelected;
            this.info = info;
            this.message = message;
            this.type = type;
        }
    }

    public class GameLoggerEditor : EditorWindow
    {
        private Rect upperPanel;
        private Rect lowerPanel;
        private Rect resizer;
        private Rect menuBar;

        private float sizeRatio = 0.5f;
        private bool isResizing;

        private float resizerHeight = 5f;
        private float menuBarHeight = 20f;
        private float channelHeight = 5f;

        private bool collapse = false;
        private bool clearOnPlay = false;
        private bool errorPause = false;
        private bool showLog = false;
        private bool showWarnings = false;
        private bool showErrors = false;

        private Vector2 upperPanelScroll;
        private Vector2 lowerPanelScroll;

        private GUIStyle resizerStyle;
        private GUIStyle boxStyle;
        private GUIStyle textAreaStyle;


        private Texture2D boxBgOdd;
        private Texture2D boxBgEven;
        private Texture2D boxBgSelected;
        private Texture2D icon;

        private Texture2D errorIcon;
        private Texture2D errorIconSmall;
        private Texture2D warningIcon;
        private Texture2D warningIconSmall;
        private Texture2D infoIcon;
        private Texture2D infoIconSmall;

        private List<Log> logs;
        private Log selectedLog;

        [MenuItem("Window/Bloktopia Console")]
        private static void OpenWindow()
        {
            GameLoggerEditor window = GetWindow<GameLoggerEditor>();
            window.titleContent = new GUIContent("Bloktopia Console");
        }

        private void OnEnable()
        {
                //iconInfoMono = EditorGUIUtility.LoadIcon("console.infoicon.inactive.sml");
                //iconWarnMono = EditorGUIUtility.LoadIcon("console.warnicon.inactive.sml");
                //iconErrorMono = EditorGUIUtility.LoadIcon("console.erroricon.inactive.sml");

            errorIcon = EditorGUIUtility.FindTexture("d_console.erroricon.sml");
            warningIcon = EditorGUIUtility.FindTexture("d_console.warnicon.sml");
            infoIcon = EditorGUIUtility.FindTexture("d_console.infoicon.sml");


            //    errorIcon = EditorGUIUtility.Load("icons/console.erroricon.png") as Texture2D;
            //warningIcon = EditorGUIUtility.Load("icons/console.warnicon.png") as Texture2D;
            //infoIcon = EditorGUIUtility.Load("icons/console.infoicon.png") as Texture2D;

            //errorIconSmall = EditorGUIUtility.Load("icons/console.erroricon.sml.png") as Texture2D;
            //warningIconSmall = EditorGUIUtility.Load("icons/console.warnicon.sml.png") as Texture2D;
            //infoIconSmall = EditorGUIUtility.Load("icons/console.infoicon.sml.png") as Texture2D;

            errorIconSmall = EditorGUIUtility.FindTexture("d_console.erroricon.sml");
            warningIconSmall = EditorGUIUtility.FindTexture("d_console.warnicon.sml");
            infoIconSmall = EditorGUIUtility.FindTexture("d_console.infoicon.sml");

            resizerStyle = new GUIStyle();
            resizerStyle.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;

            boxStyle = new GUIStyle();
            boxStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

            boxBgOdd = EditorGUIUtility.Load("builtin skins/darkskin/images/cn entrybackodd.png") as Texture2D;
            boxBgEven = EditorGUIUtility.Load("builtin skins/darkskin/images/cnentrybackeven.png") as Texture2D;
            boxBgSelected = EditorGUIUtility.Load("builtin skins/darkskin/images/menuitemhover.png") as Texture2D;

            textAreaStyle = new GUIStyle();
            textAreaStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            textAreaStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/projectbrowsericonareabg.png") as Texture2D;

            logs = new List<Log>();
            selectedLog = null;

            Application.logMessageReceived += LogMessageReceived;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= LogMessageReceived;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= LogMessageReceived;
        }

        private void LogMessageReceived(string condition, string stackTrace, LogType type)
        {
            Log l = new Log(false, condition, stackTrace, type);
            logs.Add(l);
        }

        private void OnGUI()
        {
            DrawMenuBar();
            DrawUpperPanel();
            DrawChannels();
            DrawLowerPanel();
            DrawResizer();
            

            //DrawChannelsDropdown();

            ProcessEvents(Event.current);

            if (GUI.changed) Repaint();
        }

        private void DrawMenuBar()
        {
            menuBar = new Rect(0, 0, position.width, menuBarHeight);

            GUILayout.BeginArea(menuBar, EditorStyles.toolbar);

            GUILayout.BeginHorizontal();

            if(GUILayout.Button(new GUIContent("Clear"), EditorStyles.toolbarButton, GUILayout.Width(55)))
            {
                logs.Clear();
            }
            GUILayout.Space(5);

            collapse = GUILayout.Toggle(collapse, new GUIContent("Collapse"), EditorStyles.toolbarButton, GUILayout.Width(60));
            clearOnPlay = GUILayout.Toggle(clearOnPlay, new GUIContent("Clear On Play"), EditorStyles.toolbarButton, GUILayout.Width(100));
            errorPause = GUILayout.Toggle(errorPause, new GUIContent("Error Pause"), EditorStyles.toolbarButton, GUILayout.Width(80));

            GUILayout.FlexibleSpace();

            showLog = GUILayout.Toggle(showLog, new GUIContent("L", infoIconSmall), EditorStyles.toolbarButton, GUILayout.Width(40));
            showWarnings = GUILayout.Toggle(showWarnings, new GUIContent("W", warningIconSmall), EditorStyles.toolbarButton, GUILayout.Width(40));
            showErrors = GUILayout.Toggle(showErrors, new GUIContent("E", errorIconSmall), EditorStyles.toolbarButton, GUILayout.Width(40));


            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void DrawUpperPanel()
        {
            upperPanel = new Rect(0, (channelHeight+menuBarHeight*2), position.width, (position.height * sizeRatio) - (channelHeight+ menuBarHeight*2));

            GUILayout.BeginArea(upperPanel);
            upperPanelScroll = GUILayout.BeginScrollView(upperPanelScroll);

            for (int i = 0; i < logs.Count; i++)
            {
                if (DrawBox(logs[i].info, logs[i].type, i % 2 == 0, logs[i].isSelected))
                {
                    if (selectedLog != null)
                    {
                        selectedLog.isSelected = false;
                    }

                    logs[i].isSelected = true;
                    selectedLog = logs[i];
                    GUI.changed = true;
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        HashSet<string> CurrentChannels = new HashSet<string>();
        string CurrentChannel = null;
        Vector2 DrawPos;

        List<string> GetChannels()
        {

            CurrentChannels = new HashSet<string>();
            var categories = CurrentChannels;

            var channelList = new List<string>();

            //foreach (KeyValuePair<string, GameLogger> keyValuePair in GameLog.AllLoggers)
            //{
            //    Debug.Log(keyValuePair.Key);
            //    channelList.Add(keyValuePair.Key);
            //}

            channelList.Add("All");
            channelList.Add("Network");
            //channelList.Add("Animation");
            //channelList.Add("General");
            channelList.Add("UI");

            channelList.AddRange(categories);
            return channelList;
        }

        /*
        void DrawChannelsDropdown()
        {
            if(EditorGUILayout.DropdownButton(new GUIContent("Drop"), FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Drop One"), clearOnPlay, () => { Debug.Log("Drop Down"); });
                menu.AddItem(new GUIContent("Drop Two"), clearOnPlay, () => { Debug.Log("Drop Two"); });
                var rect = GUILayoutUtility.GetLastRect();
                rect.y += EditorGUIUtility.singleLineHeight;
                menu.DropDown(rect);
            }
        }
        */

        void DrawChannels()
        {
            var channels = GetChannels();
            //Debug.Log("Channels count is :" + channels.Count);
            if (channels.Count > 0)
            {
                int currentChannelIndex = 0;
                for (int c1 = 0; c1 < channels.Count; c1++)
                {
                    if (channels[c1] == CurrentChannel)
                    {
                        currentChannelIndex = c1;
                        break;
                    }
                }

                var content = new GUIContent("S");
                var size = GUI.skin.button.CalcSize(content);

                var drawRect = new Rect(new Vector2(0, menuBarHeight), new Vector2(position.width, size.y));
                currentChannelIndex = GUI.SelectionGrid(drawRect, currentChannelIndex, channels.ToArray(), channels.Count);
                if (CurrentChannel != channels[currentChannelIndex])
                {
                    CurrentChannel = channels[currentChannelIndex];
                    //ClearSelectedMessage();
                    //MakeDirty = true;
                }
                DrawPos.y += size.y;
            }

            
        }

        private void DrawLowerPanel()
        {
            lowerPanel = new Rect(0, (position.height * sizeRatio) + resizerHeight, position.width, (position.height * (1 - sizeRatio)) - resizerHeight);

            GUILayout.BeginArea(lowerPanel);
            lowerPanelScroll = GUILayout.BeginScrollView(lowerPanelScroll);

            if (selectedLog != null)
            {
                GUILayout.TextArea(selectedLog.message, textAreaStyle);
            }



            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawResizer()
        {
            resizer = new Rect(0, (position.height * sizeRatio) - resizerHeight, position.width, resizerHeight * 2);

            GUILayout.BeginArea(new Rect(resizer.position + (Vector2.up * resizerHeight), new Vector2(position.width, 2)), resizerStyle);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(resizer, MouseCursor.ResizeVertical);
        }

        private bool DrawBox(string content, LogType boxType, bool isOdd, bool isSelected)
        {
            if (isSelected)
            {
                boxStyle.normal.background = boxBgSelected;
            }
            else
            {
                if (isOdd)
                {
                    boxStyle.normal.background = boxBgOdd;
                }
                else
                {
                    boxStyle.normal.background = boxBgEven;
                }
            }

            switch (boxType)
            {
                case LogType.Error: icon = errorIcon; break;
                case LogType.Exception: icon = errorIcon; break;
                case LogType.Assert: icon = errorIcon; break;
                case LogType.Warning: icon = warningIcon; break;
                case LogType.Log: icon = infoIcon; break;
            }

            return GUILayout.Button(new GUIContent(content, icon), boxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(30));
        }

        private void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0 && resizer.Contains(e.mousePosition))
                    {
                        isResizing = true;
                    }
                    break;

                case EventType.MouseUp:
                    isResizing = false;
                    break;
            }

            Resize(e);
        }

        private void Resize(Event e)
        {
            if (isResizing)
            {
                sizeRatio = e.mousePosition.y / position.height;
                Repaint();
            }
        }
    }
}