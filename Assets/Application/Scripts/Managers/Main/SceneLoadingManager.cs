using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Provides functionality for scene loading, retrieving the scene either from the build via name or from the <see cref="AssetBundleLoader"/> via guid
/// </summary>
/// <remarks>
/// This class should be used as the primary source of scene loading within bloktopia. It provides functionality to load scenes via name (build settings)
/// or to retrieve a scene as an AssetBundle; loading the scene upon successful retrieval.
///
/// <example>
/// A very basic use case of the class would be to load a new floor within Bloktopia and can be used in a similar light to <see cref="SceneManager"/>
/// <code>
/// private void ChangeFloor(string floorName) // where floorName is synonymous with the SceneName in the buildSettings
/// {
///     SceneLoadingManager.Instance.UnloadScene(floorName, () => Debug.Log("Scene Successfully Unloaded");
///     SceneLoadingManager.Instance.CreateSceneFromBuildSettings(floorName);
/// }
/// </code>
/// 
/// to note, when loading a new scene the previous/existing scenes are not unloaded hence why in the above example <see cref="UnloadScene"/> is called prior to
/// loading the next scene. 
/// </example>
///
/// This class is heavily relied upon by the <see cref="LoadingManager"/> and care must be taken when modifying any public facing APIs as this class
/// is a core component within the Bloktopia Scene Loading System. 
/// </remarks>
public class SceneLoadingManager : MonoSingleton<SceneLoadingManager>
{ 
    /// <summary>
    /// Action Invoked on completed scene loading
    /// </summary>
    public event Action<float> OnUpdateLoadingAmount;

    private readonly List<string> _currentLoadedSceneNames = new();
    private readonly Dictionary<string, string[]> _sceneNameCacheDict = new();
    
    /// <summary>
    /// Retrieves the scene via guid through the <see cref="AssetBundleLoader"/> and loads the scene asynchronously
    /// </summary>
    /// <remarks>
    /// This method is specifically built for loading scenes from an asset bundle using the provided guide. It first
    /// checks the <see cref="_sceneNameCacheDict"/> to see if the scene already exists before loading the scene using the
    /// provided callback. The callback is then brought for use with a <see cref="TaskAction"/> to be invoked foreach scene
    /// loaded. If the loaded asset is null, an Error is logged to the console with the guid of the scene that was attempted to
    /// load, invoking the <see cref="onError"/> Action that is passed into the function during the process.
    /// </remarks>
    /// <param name="assetGuid">the guid of the scene asset to retrieve from the <see cref="AssetBundleLoader"/></param>
    /// <param name="callback">the callback of to be invoked upon scene loaded</param>
    /// <param name="onError">the error handling Action for if the bundle load fails</param>
    public void LoadSceneFromAssetBundle(string assetGuid, Action callback, Action onError)
    {
        string[] scenes = null;

        if (_sceneNameCacheDict.ContainsKey(assetGuid))
        {
            scenes = _sceneNameCacheDict[assetGuid];
            LoadScenes(scenes, callback);
        }
        else
        {
            AssetBundleLoader.Instance.LoadSceneBundle(assetGuid, asset => 
            {
                if (asset == null)
                {
                    Debug.LogError($"This asset is null {assetGuid}");
                    onError?.Invoke();
                    return;
                }

                scenes = asset.GetAllScenePaths();
                _sceneNameCacheDict[assetGuid] = scenes;
                LoadScenes(scenes, callback);
            });
        }
    }

    /// <summary>
    /// Loads the scene via <see cref="name"/> passed in as a parameter. <see cref="callback"/> is invoked on
    /// successful scene load 
    /// </summary>
    /// <param name="name">name of the scene to be loaded</param>
    /// <param name="callback">callback to be invoked on successful load</param>
    public void CreateSceneFromBuildSettings(string name, Action callback)
    {
        LoadScene(name, sceneName =>
        {
            callback?.Invoke();
        });
    }

    /// <summary>
    /// unloads the scene via provided <see cref="name"/> passed in as a parameter. <see cref="callback"/> is invoked on
    /// successful scene unload 
    /// </summary>
    /// <param name="name">name of the scene to unload</param>
    /// <param name="callback">callback to be invoked on successful unload</param>
    public void UnloadScene(string name, Action callback)
    {
        var scene = SceneManager.GetSceneByName(name);

        if (!string.IsNullOrEmpty(scene.name))
        {
            this.WaitForOp(SceneManager.UnloadSceneAsync(scene), x => callback?.Invoke());
        }
    }

    /// <summary>
    /// Unloads all of the current loaded scenes. If there are no scenes loaded the callback is invoked regardless. 
    /// </summary>
    /// <param name="callback">Action to invoke upon all scenes unloaded</param>
    public void UnloadCurrentLoadedScene(Action callback)
    {
        if (_currentLoadedSceneNames.Count == 0)
        {
            callback?.Invoke();
            return;
        }

        var task = new TaskAction(_currentLoadedSceneNames.Count, () =>
        {
            _currentLoadedSceneNames.Clear();
            callback?.Invoke();
        });

        foreach (var sceneName in _currentLoadedSceneNames)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid())
            {
                this.WaitForOp(SceneManager.UnloadSceneAsync(scene), op =>
                {
                    task?.Increment();
                });
            }
            else
            {
                Debug.LogError($"Attempted to unload invalid scene {sceneName}");
                task?.Increment();
            }
        }
    }
    
    private void LoadScenes(string[] scenes, Action callback)
    {
        TaskAction task = new TaskAction(scenes.Length, callback);

        for (int i = 0; i < scenes.Length; i++)
        {
            var sceneName = scenes[i];
            LoadScene(sceneName, loaded =>
            {
                var fileName = Utilities.IO.GetFileName(sceneName);
                _currentLoadedSceneNames.Add(fileName);
                OnUpdateLoadingAmount?.Invoke(1f);
                task.Increment();
            });
        }
    }
    
    private void LoadScene(string sceneName, Action<Scene> callback)
    {
        this.WaitForOp(SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive), op =>
        {
            this.WaitForFrame(() => callback(SceneManager.GetSceneByName(sceneName)));
        });
    }
}
