using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.CompilerServices.SymbolWriter;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ContentDataEditorWindow : BaseEditorWindow
{
    private ContentDataEntry currentData;
    private Type currentContentType;

    private List<Type> contentTypes;
    private Vector3 contentScrollView;
    private Vector3 selectedScrollView;
    private List<Type> moduleTypes;

    private int tabCount = 0;

    private string search;
    
    private int selectedModule;

    private static void AddToStates(int key)
    {
        if (!tabActiveStates.ContainsKey(key))
            tabActiveStates.Add(key, false);
    }

    private static Dictionary<int, bool> tabActiveStates = new Dictionary<int, bool>();

    private static Dictionary<string, string> assetPaths = new Dictionary<string, string>();
    private static Dictionary<string, Object> assets = new Dictionary<string, Object>();
    private static Dictionary<string, Object> source = new Dictionary<string, Object>();

    private protected override void OnEnable()
    {
        base.OnEnable();
        contentTypes = new List<Type>
        {
            typeof(MediaEntry), typeof(SceneGraphEntry), typeof(LocationDataEntry), typeof(EnvironmentDataEntry), typeof(TemplateDataEntry)
        };
        currentContentType = contentTypes[0];

        moduleTypes = ModuleFactory.GetTypesList();
    }

    protected internal override void Refresh()
    {
        base.Refresh();
    }

    protected internal override void ManualOnGUI()
    {
        base.ManualOnGUI();
        Head();
        Body();
    }

    public void Head()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        foreach (var contentType in contentTypes)
        {
            if (GUILayout.Button(contentType.ToString(), GUILayout.Width(EditorConsts.SHORT_BUTTON_WIDTH)))
            {
                currentContentType = contentType;
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        foreach (var contentType in contentTypes)
        {
            if (GUILayout.Button($"Create {contentType.ToString()}", GUILayout.Width(EditorConsts.SHORT_BUTTON_WIDTH)))
            {
                ContentDataEntry contentDataEntry = (ContentDataEntry)Activator.CreateInstance(contentType);
                contentDataEntry.guid = Guid.NewGuid().ToString();
                contentDataEntry.name = contentType.ToString();
                currentData = contentDataEntry;
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    public void Body()
    {
        tabCount = 0;

        EditorGUILayout.BeginHorizontal();
        DrawContentList();
        ShowItem(currentData);
        EditorGUILayout.EndHorizontal();
    }

    void DrawContentList()
    {
        contentScrollView = EditorGUILayout.BeginScrollView(contentScrollView,
            false,
            true,
            GUILayout.ExpandHeight(true),
            GUILayout.MaxWidth(EditorConsts.SCROLL_WIDTH));

        EditorGUILayout.BeginVertical(GUILayout.Width(EditorConsts.SEARCH_BOX),GUILayout.MinHeight(1000));

        search = GUILayout.TextField(search,
            GUILayout.Width(EditorConsts.CONTENT_DATA_WIDTH),
            GUILayout.Height(EditorConsts.CONTENT_SEARCH_HEIGHT));
        
        foreach (var data in ContentManager.Instance.DictOfDataEntries)
        {
            if (data.Value == null)
            {
                Debug.LogError("data is null");
                continue;
            }

            if (data.Value.GetType() != currentContentType)
                continue;

            if (search != null)
            {
                if (!data.Value.name.ToLower().Contains(search) && !data.Value.guid.ToLower().Contains(search))
                {
                    continue;
                }
            }

            if (GUILayout.Button($"{data.Value.name}-{data.Value.guid}",
                    GUILayout.Width(EditorConsts.CONTENT_DATA_WIDTH),
                    GUILayout.Height(EditorConsts.CONTENT_DATA_HEIGHT)))
            {
                tabActiveStates.Clear();
                currentData = data.Value;
            }
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    void ShowItem(ContentDataEntry currentData)
    {
        selectedScrollView = EditorGUILayout.BeginScrollView(selectedScrollView,
            false,
            true,
            GUILayout.ExpandHeight(true),
            GUILayout.MaxWidth(EditorConsts.SCROLL_WIDTH));


        GUILayout.BeginVertical(GUILayout.MinHeight(1000));
        if (currentData != null)
        {
            PropertyField(currentData, currentData.GetType(), null, out var result, currentData.ToString());
            UploadData(currentData);
            UpdateAsset(currentData);
        }
        
        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private void UpdateAsset(ContentDataEntry dataEntry)
    {
        if (dataEntry is MediaEntry mediaEntry)
        {
            if (GUILayout.Button("Update Asset"))
            {
                AssetBundler.ShowAssetCreator(mediaEntry);
            }
        }
    }
    
    private void UploadData(ContentDataEntry dataEntry)
    {
        if (ContentManager.Instance.DictOfDataEntries.ContainsKey(currentData.guid))
        {
            UploadData("Update data", dataEntry, WebRequestManager.UploadMethod.PUT);
        }
        else
        {
            UploadData("Upload data", dataEntry, WebRequestManager.UploadMethod.POST);
        }
    }
    
    private void UploadData(string buttonText, ContentDataEntry dataEntry, WebRequestManager.UploadMethod method)
    {
        if (GUILayout.Button(buttonText))
        {
            if (DataValidationCheck(dataEntry))
            {
                if (dataEntry is TemplateDataEntry templateDataEntry)
                {
                    templateDataEntry.dependencyList.Clear();
                    if (!string.IsNullOrWhiteSpace(templateDataEntry.sourceContentDataEntryGuid))
                    {
                        foreach (var moduleData in templateDataEntry.moduleData)
                        {
                            foreach (var dependency in moduleData.GetDependencyData())
                            {
                                if (!templateDataEntry.dependencyList.Contains(dependency))
                                {
                                    templateDataEntry.dependencyList.Add(dependency);
                                }
                            }
                        }
                        
                        if (!templateDataEntry.dependencyList.Contains(templateDataEntry.sourceContentDataEntryGuid))
                        {
                            templateDataEntry.dependencyList.Add(templateDataEntry.sourceContentDataEntryGuid);
                        }
                        PostData(dataEntry, method);
                    }
                }
                else
                {
                    PostData(dataEntry, method);
                }
            }
        }
    }

    private void PostData(ContentDataEntry dataEntry, WebRequestManager.UploadMethod method)
    {
        WebRequestManager.Instance.UploadSmallItem(DomainManager.Instance.ContentDataEndPoint(), method,
            dataEntry, () =>
            {
                Debug.Log($"Successful uploading of data {dataEntry}");

                if (!ContentManager.Instance.DictOfDataEntries.ContainsKey(dataEntry.guid))
                {
                    ContentManager.Instance.DictOfDataEntries.Add(dataEntry.guid, dataEntry);
                }
            }, s => { Debug.LogError($"Failed to upload data {dataEntry} response is {s}");}
        );
    }
    
    private bool DataValidationCheck(ContentDataEntry dataEntry)
    {
        if (string.IsNullOrEmpty(dataEntry.guid) || string.IsNullOrEmpty(dataEntry.name))
        {
            Debug.LogError($"Guid or name is not right for the data");
            return false;
        }
            
        switch (dataEntry)
        {
            case MediaEntry mediaEntry:
                if (string.IsNullOrEmpty(mediaEntry.assetData.downloadData))
                {
                    Debug.LogError($"Enter download guid or data");
                    return false;
                }

                if (mediaEntry.assetData.assetHashDict == null || mediaEntry.assetData.assetHashDict.Count == 0)
                {
                    if (mediaEntry.media == Media.Video)
                    {
                        return true;
                    }
                    Debug.LogError($"Check hash data");
                    return false;
                }
                break;
            case TemplateDataEntry templateDataEntry:
                if (string.IsNullOrEmpty(templateDataEntry.sourceContentDataEntryGuid))
                {
                    Debug.LogError($"Enter valid source guid");
                    return false;
                }
                break;
            case LocationDataEntry locationDataEntry:
                if (locationDataEntry.scenes.Count == 0)
                {
                    Debug.LogError($"Check list of scenes has at least 1");
                    return false;
                }
                break;
            case EnvironmentDataEntry environmentDataEntry:
                if (string.IsNullOrEmpty(environmentDataEntry.sceneGraph) ||
                    string.IsNullOrEmpty(environmentDataEntry.location))
                {
                    Debug.LogError($"Either scene graph or location guid is not right");
                    return false;
                }
                break;
        }

        return true;
    }
    
    private static object NewPrimitive(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return string.Empty;
    }

    private string[] ModuleToString()
    {
        List<string> modules = new List<string>();
        
        foreach (var moduleType in moduleTypes)
        {
            modules.Add(moduleType.ToString());
        }
        
        return modules.ToArray();
    }
    
    private bool AttributeField(object value, Type type, FieldInfo customAttributeData, out object result, string label)
    {
        var attributes = Attribute.GetCustomAttribute(type, typeof(TemplateAttribute));

        if (attributes != null)
        {
            var templateData = (TemplateDataEntry) value;

            EditorGUILayout.BeginHorizontal();
            
            selectedModule = EditorGUILayout.Popup(selectedModule, ModuleToString(), GUILayout.Width(EditorConsts.SHORT_BUTTON_WIDTH));
            
            if(GUILayout.Button("Add selected module", GUILayout.Width(EditorConsts.SHORT_BUTTON_WIDTH)))
            {
                BaseModuleData contentDataEntry = (BaseModuleData)Activator.CreateInstance(moduleTypes[selectedModule]);
                
                templateData.moduleData.Add(contentDataEntry);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        result = value;
        return false;
    }

    private void PropertyField(object value, Type type, FieldInfo fieldInfo, out object result, string label)
    {
        if (value == null)
        {
            try
            {
                if (type.GetConstructors().Any(t => !t.GetParameters().Any()))
                    value = Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                Debug.LogError($"{type}--{e}");
            }
        }

        if (AttributeField(value, type, fieldInfo, out result, label))
        {
            return;
        }

        if (type == typeof(float))
        {
            result = EditorGUILayout.FloatField(label, (float)value, GUILayout.Width(EditorConsts.CONTENT_DATA_WIDTH));
            return;
        }

        if (type == typeof(double))
        {
            result = EditorGUILayout.DoubleField(label,
                (double)value,
                GUILayout.Width(EditorConsts.CONTENT_DATA_WIDTH));
            return;
        }

        if (type == typeof(int))
        {
            result = EditorGUILayout.IntField(label, (int)value, GUILayout.Width(EditorConsts.CONTENT_DATA_WIDTH));
            return;
        }

        if (type == typeof(Color))
        {
            result = EditorGUILayout.ColorField(label, (Color)value, GUILayout.Width(EditorConsts.CONTENT_DATA_WIDTH));
            return;
        }

        if (type == typeof(string))
        {
            result = EditorGUILayout.TextField(label, (string)value, GUILayout.Width(EditorConsts.CONTENT_DATA_WIDTH));
            return;
        }

        if (type == typeof(bool))
        {
            result = EditorGUILayout.Toggle(label, (bool)value, GUILayout.Width(EditorConsts.CONTENT_DATA_WIDTH));
            return;
        }

        if (type == typeof(Vector3))
        {
            result = EditorGUILayout.Vector3Field(label,
                (Vector3)value,
                GUILayout.Width(EditorConsts.CONTENT_DATA_WIDTH));
            return;
        }

        if (type == typeof(Vector2))
        {
            result = EditorGUILayout.Vector2Field(label,
                (Vector2)value,
                GUILayout.Width(EditorConsts.CONTENT_DATA_WIDTH));
            return;
        }

        if (type == typeof(Quaternion))
        {
            var rotation = ((Quaternion)value).eulerAngles;
            rotation = EditorGUILayout.Vector3Field(label,
                (Vector3)rotation,
                GUILayout.Width(EditorConsts.CONTENT_DATA_WIDTH));
            result = Quaternion.Euler(rotation);
            return;
        }

        if (type.IsEnum)
        {
            result = EditorGUILayout.EnumPopup(label, (Enum)value, GUILayout.Width(EditorConsts.CONTENT_DATA_WIDTH));
            return;
        }

        if (type.IsArray)
        {
            EditorGUI.indentLevel++;
            if (value == null)
            {
                Debug.Log("Value is null for type: " + type.GetElementType());
                value = Array.CreateInstance(type.GetElementType(), 0);
            }

            for (int i = 0; i < ((Array)value).Length; i++)
            {
                PropertyField(((Array)value).GetValue(i), type.GetElementType(), null, out var r, "");
                ((Array)value).SetValue(r, i);
            }


            GUIStyle indentedLevelStyle = new GUIStyle(GUI.skin.box);
            indentedLevelStyle.margin = new RectOffset(EditorGUI.indentLevel * 15, 0, 0, 0);

            EditorGUILayout.BeginHorizontal(indentedLevelStyle);
            if (GUILayout.Button("+", GUILayout.Width(EditorConsts.CLOSE_BUTTON_WIDTH)))
            {
                var newArray = Array.CreateInstance(type.GetElementType(), ((Array)value).Length + 1);
                Array.Copy(((Array)value), newArray, ((Array)value).Length);
                value = newArray;
            }

            if (GUILayout.Button("-", GUILayout.Width(EditorConsts.CLOSE_BUTTON_WIDTH)))
            {
                var newArray = Array.CreateInstance(type.GetElementType(), ((Array)value).Length - 1);
                Array.Copy(((Array)value), newArray, ((Array)value).Length - 1);
                value = newArray;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
            result = value;
            return;
        }

        if (type.IsGenericType)
        {
            if (type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
            {
                tabCount++;
                AddToStates(tabCount);

                tabActiveStates[tabCount] = EditorGUILayout.Foldout(tabActiveStates[tabCount], $"{label}", true);

                if (!tabActiveStates[tabCount])
                    return;

                GUIStyle indentedLevelStyle = new GUIStyle();
                indentedLevelStyle.margin = new RectOffset(EditorGUI.indentLevel * 15, 0, 0, 0);

                EditorGUILayout.BeginHorizontal(indentedLevelStyle);
                GUILayout.Label(label);
                EditorGUILayout.EndHorizontal();


                EditorGUI.indentLevel++;
                var listContentType = value.GetType().GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(listContentType);

                //need to recreate list to edit it
                var list = (IList)Activator.CreateInstance(listType);
                foreach (var i in (IEnumerable)value)
                {
                    list.Add(i);
                }

                indentedLevelStyle = new GUIStyle(GUI.skin.box);
                indentedLevelStyle.margin = new RectOffset(EditorGUI.indentLevel * 15, 0, 0, 0);

                for (var index = 0; index < list.Count; index++)
                {
                    EditorGUILayout.BeginHorizontal(indentedLevelStyle);
                    if (GUILayout.Button("+", GUILayout.Width(EditorConsts.CLOSE_BUTTON_WIDTH)))
                    {
                        list.Insert(index, null);
                        EditorGUILayout.EndHorizontal();
                        continue;
                    }

                    if (GUILayout.Button("-", GUILayout.Width(EditorConsts.CLOSE_BUTTON_WIDTH)))
                    {
                        if (list.Count > index)
                        {
                            list.RemoveAt(index);
                            EditorGUILayout.EndHorizontal();
                            continue;
                        }
                    }

                    if (GUILayout.Button("^", GUILayout.Width(EditorConsts.CLOSE_BUTTON_WIDTH)))
                    {
                        if (index > 0)
                        {
                            var element = list[index];
                            list.RemoveAt(index);
                            list.Insert(index - 1, element);
                            EditorGUILayout.EndHorizontal();
                            continue;
                        }
                    }

                    if (GUILayout.Button("˅", GUILayout.Width(EditorConsts.CLOSE_BUTTON_WIDTH)))
                    {
                        if (list.Count - 1 > index)
                        {
                            var element = list[index];
                            list.RemoveAt(index);
                            list.Insert(index + 1, element);
                            EditorGUILayout.EndHorizontal();
                            continue;
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    PropertyField(list[index], listContentType, null, out dynamic r, listContentType.ToString());
                    list[index] = r;
                }

                EditorGUI.indentLevel--;

                if (list.Count == 0)
                {
                    EditorGUILayout.BeginHorizontal(indentedLevelStyle);
                    if (GUILayout.Button("+", GUILayout.Width(EditorConsts.CLOSE_BUTTON_WIDTH)))
                    {
                        list.Add(null);
                    }

                    if (GUILayout.Button("-", GUILayout.Width(EditorConsts.CLOSE_BUTTON_WIDTH)))
                    {
                        if (list.Count > 0)
                            list.RemoveAt(list.Count - 1);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                value = list;
                result = value;
                return;
            }

            if (type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
            {
                EditorGUI.indentLevel++;

                var dictContentType = value.GetType().GetGenericArguments();

                //make a copy dict to edit the original dict
                var dictType = typeof(Dictionary<,>);
                var dictCache = (IDictionary)Activator.CreateInstance(dictType.MakeGenericType(dictContentType));
                foreach (DictionaryEntry pair in (IDictionary)value)
                {
                    dictCache.Add(pair.Key, pair.Value);
                }

                //make lists to display and edit the data
                var listType = typeof(List<>);
                var keysList = (IList)Activator.CreateInstance(listType.MakeGenericType(dictContentType[0]));
                var valuesList = (IList)Activator.CreateInstance(listType.MakeGenericType(dictContentType[1]));

                foreach (DictionaryEntry pair in dictCache)
                {
                    keysList.Add(pair.Key);
                    valuesList.Add(pair.Value);
                }


                GUIStyle indentedLevelStyle = new GUIStyle(GUI.skin.box);
                GUIStyle indentButton = new GUIStyle(GUI.skin.button);

                indentedLevelStyle.margin = new RectOffset(EditorGUI.indentLevel, 0, 0, 0);
                indentButton.margin = new RectOffset(EditorGUI.indentLevel * 10, 0, 0, 0);

                EditorGUILayout.BeginVertical(indentedLevelStyle);

                for (int i = keysList.Count - 1; i > -1; i--)
                {
                    if (GUILayout.Button("x", indentButton, GUILayout.Width(EditorConsts.GOTO_BUTTON_HEIGHT)))
                    {
                        keysList.RemoveAt(i);
                        valuesList.RemoveAt(i);
                        break;
                    }

                    PropertyField(keysList[i],
                        keysList[i].GetType(),
                        null,
                        out var k,
                        $"Key ({keysList[i].GetType().Name})");
                    PropertyField(valuesList[i],
                        valuesList[i].GetType(),
                        null,
                        out var v,
                        $"Value ({valuesList[i].GetType().Name})");

                    keysList[i] = k;
                    valuesList[i] = v;

                    EditorGUILayout.Space(5);
                }

                dictCache.Clear();

                for (int i = 0; i < keysList.Count; i++)
                {
                    dictCache.Add(keysList[i], valuesList[i]);
                }

                if (GUILayout.Button("+", indentButton, GUILayout.Width(EditorConsts.GOTO_BUTTON_HEIGHT)))
                {
                    dictCache.Add(NewPrimitive(dictContentType[0]), NewPrimitive(dictContentType[1]));
                }

                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel--;
                value = dictCache;
                result = value;
                return;
            }

            if (type.GetGenericTypeDefinition().IsAssignableFrom(typeof(KeyValuePair<,>)))
            {
                PropertyField((KeyValuePair<string, string>)value, typeof(string), null, out var r, "");
                result = value;
                return;
            }
        }

        if (type == typeof(Type))
        {
            Debug.Log($"Type not handled ");
            result = value;
            return;
        }

        if (type.IsClass)
        {
            if (value != null)
            {
                EditorGUI.indentLevel++;

                var indentedLevelStyle = new GUIStyle(GUI.skin.box);
                indentedLevelStyle.margin = new RectOffset(EditorGUI.indentLevel * 10, 0, 0, 0);

                EditorGUILayout.BeginHorizontal(indentedLevelStyle);
                GUILayout.Label(label);
                EditorGUILayout.EndHorizontal();

                GUILayout.BeginVertical(indentedLevelStyle);

                var fields = value.GetType().GetFields();
                for (var index = 0; index < fields.Length; index++)
                {
                    var field = fields[index];
                    PropertyField(field.GetValue(value), field.FieldType, field, out var r, field.Name);
                    field.SetValue(value, r);
                }

                GUILayout.EndVertical();

                EditorGUI.indentLevel--;
                result = value;
                return;
            }
        }

        Debug.Log($"Unknown property for {label} {type}");
        GUILayout.Label($"{label} - {value}");
        result = value;
    }
}