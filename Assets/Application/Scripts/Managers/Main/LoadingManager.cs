using System;
using UnityEngine;

/// <summary>
/// Responsible for the loading and unloading of environments and scene graphs. 
/// </summary>
/// <remarks>
/// The LoadingManager Singleton acts as a central manager for 2 other classes: <see cref="SceneLoadingManager"/> and <see cref="SceneGraphManager"/>.
/// It communicates between the 2 classes to load and unload specific data and scenes.
/// </remarks>
public class LoadingManager : MonoSingleton<LoadingManager>
{
    /// <summary>
    /// An enum to keep track of the loading state.
    /// When an environment is being changed or loaded via <see cref="LoadingManager.ChangeEnvironment"/> or <see cref="LoadingManager.LoadEnvironment"/> it is set to <see cref="LoadingEnvironment"/>.
    /// Once an environment has been loaded, it is set to <see cref="LoadingPost"/>.
    /// </summary>
    public enum LoadingState
    {
        Loaded,
        LoadingEnvironment,
        LoadingPost,
    }

    /// <summary>
    /// Holds reference to the scenes currently in use, and the objects within them.
    /// </summary>
    public EnvironmentDataEntry activeEnvironment { private set; get; }
    
    /// <summary>
    /// Holds reference to the media entry guids that are referenced in the current scenes.
    /// </summary>
    public LocationDataEntry activeLocation { private set; get; }
    
    private LoadingState loadingState = LoadingState.Loaded;

    /// <summary>
    /// Event that is fired when the environment is loading, and is called within <see cref="LoadEnvironment"/>.
    /// </summary>
    public event Action OnLoadingEnvironment;
    
    /// <summary>
    /// Event that is fired when the environment is changing, and is called within <see cref="ChangeEnvironment"/>.
    /// </summary>
    public event Action OnLeavingEnvironment;
    
    /// <summary>
    /// Event that is fired when the environment has finished loading, and is called within <see cref="BuildComplete"/>
    /// </summary>
    public event Action OnEnvironmentLoaded;
    

    /// <summary>
    /// The Change Environment function communicates with other Singletons to create and load the required data and scenes.
    /// When a user joins a lobby, the <see cref="LobbyUI"/> class calls <see cref="ChangeEnvironment"/> with a given environment ID.
    /// This method communicates with various other Singletons: <see cref="SceneLoadingManager"/> , <see cref="SceneGraphManager"/> and <see cref="AssetBundleLoader"/> </summary>
    /// <param name="guid"> The environment ID passed through when a user joins a Lobby</param>.
    public void ChangeEnvironment(string guid)
    {
        loadingState = LoadingState.LoadingEnvironment;
        OnLeavingEnvironment?.Invoke();

        //TODO: this needs to be complete when we have a loading scene and also the right files to build environments
        SceneLoadingManager.Instance.CreateSceneFromBuildSettings("Loading", () =>
        {
            SceneGraphManager.Instance.Unload();
            SceneLoadingManager.Instance.UnloadCurrentLoadedScene(() =>
            {
                AssetBundleLoader.Instance.Reset();

                LoadEnvironment(guid);
            });
        });
    }

    private void LoadEnvironment(string guid)
    {
        OnLoadingEnvironment?.Invoke();

        ContentManager.Instance.GetData(guid, entry =>
        {
            activeEnvironment = (EnvironmentDataEntry)entry;
            ContentManager.Instance.GetData(activeEnvironment.location, entry =>
            {
                DependencyDownloader.DownloadDependencies(activeEnvironment, () =>
                {
                    activeLocation = (LocationDataEntry)entry;
                    TaskAction task = new TaskAction(activeLocation.scenes.Count, () =>
                    {
                        loadingState = LoadingState.LoadingEnvironment;
                        LightProbes.TetrahedralizeAsync();
                        SceneGraphManager.Instance.Load(activeEnvironment.sceneGraph, BuildComplete);
                    });
                    foreach (var sceneGuid in activeLocation.scenes)
                    {
                        ContentManager.Instance.GetData(sceneGuid, entry =>
                        {
                            var sceneData = (MediaEntry)entry;
                            SceneLoadingManager.Instance.LoadSceneFromAssetBundle(sceneData.assetData.downloadData,
                                () => { task.Increment(); },
                                () =>
                                {
                                    Debug.Log($"Failed to load scene {sceneData.guid}");
                                    task.Increment();
                                });
                        }, s =>
                        {
                            Debug.Log($"Failed to load scene data {sceneGuid} - {s}");
                        });
                    }
                });
            }, s =>
            {
                Debug.LogError($"Failed to load location {guid} - {s}");
            });
        }, s =>
        {
            Debug.LogError($"Failed to load environment {guid} - {s}");
        });
    }

    private void BuildComplete()
    {
        loadingState = LoadingState.LoadingPost;
        CloseLoadingScene();
        OnEnvironmentLoaded?.Invoke();
    }

    private void CloseLoadingScene()
    {
        SceneLoadingManager.Instance.UnloadScene("Loading", null);
    }
}