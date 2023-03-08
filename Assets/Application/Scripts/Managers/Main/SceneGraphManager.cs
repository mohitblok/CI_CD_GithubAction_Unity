using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

/// <summary>
/// Used to Load and unload <see cref="SceneGraphEntry"/>'s into the scene
/// </summary>
/// <remarks>
/// With Two entry points into the script <see cref="Load"/> and <see cref="Unload"/> this script handles the loading & unloading
/// the SceneGraph for each scene, but not the loading of scenes themselves. If you are looking to load scenes specifically see the <see cref="SceneLoadingManager"/>
///
/// To correctly use this system to load an environment into a <see cref="Scene"/> the guid of the specific scene-graph environment is required.
/// <example>
/// The Following Example is a very basic example of how to implement this class for scene-graph loading using <see cref="EnvironmentDataEntry"/>
/// <code>
/// private void LoadScene(EnvironmentDataEntry environment)
/// {
///     SceneGraphManager.Instance.Load(environment.sceneGraph, () => Debug.Log("Loading Complete"));
/// }
/// </code>
/// In the above example we utilise the predefined <see cref="EnvironmentDataEntry"/> class to our advantage. It would be assumed that this object exists
/// prior to the example as it can be used to store information about the Environment being loaded. 
/// </example>
///
/// Unload works in a much simpler manner. looping through the cached loaded items in the <see cref="sceneItemsDict"/> and Destroying each item stored.
///
/// Advised use for this class is to keep it closed for Modification and Closed for extension; hence the sealed class type. This script has a single use
/// and responsibility. If looking to make modifications to this class take care as the <see cref="LoadingManager"/> is dependant upon this to function as
/// expected. 
/// </remarks>
public sealed class SceneGraphManager : MonoSingleton<SceneGraphManager>
{
    private readonly Dictionary<ConfiguredItemEntry, GameObject> sceneItemsDict = new();

    /// <summary>
    /// Loads SceneGraph items into the scene, passing the provided callback to initialise each loaded gameobject
    /// </summary>
    /// <param name="guid">the guid of the scene to load</param>
    /// <param name="callback">the callback to initialise each GameObject with</param>
    /// <remarks>
    /// This method is specific to scene loading using the <see cref="ContentManager"/> and <see cref="SceneGraphEntry"/> creating
    /// the GameObjects dependant on the Entries within the SceneGraph. If the <see cref="SceneGraphEntry.sceneItems"/> fail to load
    /// a log will be thrown to inform the developer.
    /// </remarks>
    public void Load(string guid, Action callback)
    {
        sceneItemsDict.Clear();
        
        ContentManager.Instance.GetData(guid, entry =>
        {
            var sceneGraph = (SceneGraphEntry)entry;
            var task = new TaskAction(sceneGraph.sceneItems.Count, callback);
            CreateGameobjects(sceneGraph.sceneItems, () => { InitItems(callback); });
        }, s => { Debug.Log($"Failed to load scene graph {guid} - {s}"); });
    }
    
    /// <summary>
    /// Unloads all of the SceneItems (<see cref="ConfiguredItemEntry"/>) from the scene using the <see cref="sceneItemsDict"/> as a list. Clearing the list, upon completion.
    /// </summary>
    [InspectorButton]
    public void Unload()
    {
        foreach (var sceneItem in sceneItemsDict)
        {
            Destroy(sceneItem.Value);
        }

        sceneItemsDict.Clear();
    }

    private void CreateGameobjects(List<ConfiguredItemEntry> sceneItems, Action callback)
    {
        TaskAction taskAction = new TaskAction(sceneItems.Count, callback);
        foreach (var sceneItem in sceneItems)
        {
            GetDownloadData(sceneItem.contentGuid, s =>
            {
                LoadSceneItem(s, o =>
                {
                    sceneItemsDict[sceneItem] = o;
                    taskAction?.Increment();
                });
            });
        }
    }

    private void InitItems(Action initComplete)
    {
        TaskAction task = new TaskAction(sceneItemsDict.Count, initComplete);

        foreach (var pair in sceneItemsDict)
        {
            pair.Value.transform.position = pair.Key.position;
            pair.Value.transform.rotation = pair.Key.rotation;

            ContentManager.Instance.GetData(pair.Key.contentGuid, contentData =>
            {
                if (contentData is TemplateDataEntry data)
                {
                    pair.Value.ForceComponent<RootModule>().AddModules(data.moduleData, task.Increment);
                }
                else
                {
                    task.Increment();
                }
                
                pair.Value.name = pair.Key.contentGuid;
            }, s =>
            {
                task.Increment();
                Debug.Log(
                    $"Failed to load scene item {pair.Key.contentGuid} - {s}");
            });
        }
    }

    private void GetDownloadData(string contentGuid, Action<string> callback)
    {
        TemplateDataEntry templateDataEntry;
        MediaEntry mediaEntry;

        ContentManager.Instance.GetData(contentGuid, contentData =>
        {
            if (contentData is TemplateDataEntry data)
            {
                templateDataEntry = data;
                ContentManager.Instance.GetData(templateDataEntry.sourceContentDataEntryGuid,
                    dataEntry => { callback?.Invoke(((MediaEntry)dataEntry).assetData.downloadData); },
                    s =>
                    {
                        callback?.Invoke(string.Empty);
                        Debug.Log(
                            $"Failed to load template source {templateDataEntry.sourceContentDataEntryGuid} - {s}");
                    });
            }
            else
            {
                mediaEntry = (MediaEntry)contentData;
                callback?.Invoke(mediaEntry.assetData.downloadData);
            }
        }, s => { callback?.Invoke(string.Empty); });
    }

    private void LoadSceneItem(string downloadDataGuid, Action<GameObject> callback)
    {
        AssetBundleLoader.Instance.LoadBundle(downloadDataGuid, "",
            (obj) =>
            {
                if (obj == null)
                {
                    callback?.Invoke(null);
                    Debug.LogError($"Asset bundle for guid {downloadDataGuid} was null, skipping...");
                    return;
                }

                GameObject go = Instantiate((GameObject)obj, transform);
                callback?.Invoke(go);
            });
    }
}