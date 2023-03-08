using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// The "AssetBundleLoader" class is a Unity script designed to load asset bundles asynchronously.
/// Overall, the "AssetBundleLoader" class provides a convenient way to load and manage asset bundles in Unity.
/// </summary>
public class AssetBundleLoader : MonoSingleton<AssetBundleLoader>
{
    private Queue<AssetItem> AssetBundleRequestQueue = new Queue<AssetItem>();
    private Dictionary<string, Object> assetData = new Dictionary<string, Object>();
    private Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

    private void Start()
    {
        Reset();
    }
    
    /// <summary>
    /// The "LoadBundle" method takes a GUID, a name, and a callback function as parameters, which are used to load an
    /// asset bundle asynchronously. The method retrieves the local path of the asset bundle using the "DomainManager"
    /// instance and creates an "AssetItem" object that contains information about the asset bundle request. The "AssetItem"
    /// object is then added to the asset bundle request queue. The callback function is used to pass the loaded asset
    /// data to the calling function.
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    public void LoadBundle(string guid, string name, Action<Object> callback)
    {
        var path = DomainManager.Instance.GetLocalAssetBundle(guid);

        AssetItem item = new AssetItem(guid, name, path, callback);
        AssetBundleRequestQueue.Enqueue(item);
    }

    private IEnumerator LoadNext()
    {
        while (true)
        {
            if (AssetBundleRequestQueue.Count > 0)
            {
                AssetItem item = AssetBundleRequestQueue.Dequeue();
                yield return LoadAssetBundleIE(item);
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator LoadAssetBundleIE(AssetItem item)
    {
        yield return null;

        Object asset = default;

        if (!assetData.ContainsKey(item.Guid))
        {
            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(item.Path);
            yield return bundleLoadRequest;

            var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
            if (myLoadedAssetBundle == null)
            {
                Debug.LogError($"Failed to load AssetBundle {item.Path}");
                item.Callback?.Invoke(null);

                if (File.Exists(item.Path))
                {
                    Debug.LogError($"Corrupt bundle exists at {item.Path} deleting");
                    File.Delete(item.Path);
                }

                yield break;
            }

            yield return ExtractAsset(myLoadedAssetBundle, item.Name, item.Guid);
            
            if (myLoadedAssetBundle)
                myLoadedAssetBundle.Unload(false);
        }
        
        asset = assetData[item.Guid];
        item.Callback?.Invoke(asset);
    }

    /// <summary>
    /// The "LoadSceneBundle" method takes a GUID and a callback function as parameters and is used to load a scene asset
    /// bundle asynchronously. The method retrieves the local path of the asset bundle using the "DomainManager" instance
    /// and starts the "LoadSceneBundleIE" coroutine with the retrieved path and callback function. The callback function is
    /// used to pass the loaded asset data to the calling function. The method uses a coroutine to perform the asynchronous
    /// loading operation.
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="callback"></param>
    public void LoadSceneBundle(string guid, Action<AssetBundle> callback)
    {
        var path = DomainManager.Instance.GetLocalAssetBundle(guid);
        StartCoroutine(LoadSceneBundleIE(guid, path, callback));
    }

    private IEnumerator LoadSceneBundleIE(string guid, string path, Action<AssetBundle> callback)
    {
        yield return null;

        AssetBundleCreateRequest bundleLoadRequest = null;
        
        if (assetBundles.ContainsKey(guid) == false)
        {
            AssetBundle myLoadedAssetBundle = null;

            if (myLoadedAssetBundle == null)
            {
                bundleLoadRequest = AssetBundle.LoadFromFileAsync(path);
                yield return bundleLoadRequest;

                myLoadedAssetBundle = bundleLoadRequest.assetBundle;
            }

            if (myLoadedAssetBundle != null)
            {
                assetBundles.Add(guid, myLoadedAssetBundle);
            }
        }

        callback?.Invoke(bundleLoadRequest.assetBundle);
    }

    private IEnumerator ExtractAsset(AssetBundle myLoadedAssetBundle, string name, string guid)
    {
        if (string.IsNullOrEmpty(name))
        {
            var assetNameList = myLoadedAssetBundle.GetAllAssetNames();

            if (assetNameList != null && assetNameList.Length > 0)
                name = myLoadedAssetBundle.GetAllAssetNames()[0];
        }

        if (string.IsNullOrEmpty(name) == false)
        {
            var assetLoadRequest = myLoadedAssetBundle.LoadAssetAsync(name);
            yield return assetLoadRequest;
            assetData[guid] = assetLoadRequest.asset;
        }
    }

    private class AssetItem
    {
        /// <summary>
        /// Gets or sets the value of the guid
        /// </summary>
        public string Guid { get; private set; }
        /// <summary>
        /// Gets or sets the value of the name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets or sets the value of the path
        /// </summary>
        public string Path { get; private set; }
        /// <summary>
        /// Gets or sets the value of the callback
        /// </summary>
        public Action<Object> Callback { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetItem"/> class
        /// </summary>
        /// <param name="guid">The guid</param>
        /// <param name="name">The name</param>
        /// <param name="path">The path</param>
        /// <param name="callback">The callback</param>
        public AssetItem(string guid, string name, string path, Action<Object> callback)
        {
            Guid = guid;
            Name = name;
            Path = path;
            Callback = callback;
        }
    }

    /// <summary>
    /// This method resets the application state and prepares it for loading the next scene or asset bundle.
    /// It stops all currently running Coroutines, clears any loaded AssetBundles, unloads unused assets,
    /// and then starts a new Coroutine to load the next scene or asset bundle.
    /// </summary>
    public void Reset()
    {
        StopAllCoroutines();
        ClearAssetBundles();
        Resources.UnloadUnusedAssets();
        StartCoroutine(LoadNext());
    }

    private void ClearAssetBundles()
    {
        assetData.Clear();
        AssetBundleRequestQueue.Clear();
        AssetBundle.UnloadAllAssetBundles(true);
    }
}