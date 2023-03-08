using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Siccity.GLTFUtility;
using SimpleJSON;
using UnityEngine;
using Utilities;

/// <summary>
/// Using the <see cref="LoadAvatar"/> an avatar is added to a queue to be loaded via a coroutine. This coroutine runs continuously in the background of the app.
/// </summary>
/// <remarks>
/// The AvatarLoader is to be added to a GameObject or instantiated through <see cref="MonoSingleton{T}.CreateInstance"/>. Upon loading the <see cref="Start"/> method on the <see cref="MonoBehaviour"/> will
/// set the <see cref="loadNext"/> coroutine in motion that runs throughout the objects lifetime. This coroutine will check the queue for any existing items and upon finding an item will dequeue the item
/// and download the Avatar (<see cref="DownloadAvatar"/>.)   
/// 
/// The AvatarLoader has one entry point the <see cref="LoadAvatar"/> method which requires an <see cref="Avatar"/> Instance to be used. From this entry point the avatar and the provided url
/// are combined into an <see cref="AvatarItem"/> object and added to the queue.
///
/// Stepping into this system for modifications is ill-advised with it's current implementation as the system acts with a single entry point to output a desired result; (<see cref="DownloadAvatar"/>)
/// downloading the avatar, supplying it to the model through the <see cref="AvatarItem"/> <see cref="TaskAction"/> callback which supplies the downloaded lod models to the <see cref="Avatar"/>
/// </remarks>
/// <example>
/// A very basic Example of loading an Avatar using the AvatarLoader
/// <code>
///     public void CreateAvatar(string readyPlayerMeAvatarURL)
///     {
///         var avatar = gameObject.AddComponent<Avatar>();
///         AvatarLoader.Instance.Load(readyPlayerMeAvatarURL, avatar);
///     }
/// </code>
/// </example>
public class AvatarLoader : MonoSingleton<AvatarLoader>
{
    private Queue<AvatarItem> loadQueue = new Queue<AvatarItem>();
    private static AvatarItem avatarItem;

    enum Quality
    {
        low = 2,
        medium = 1,
        high = 0,
    }
    
    // todo: Stop the coroutine when Queue == 0 and restart it when the Queue > 0
    private void Start()
    {
        StartCoroutine(loadNext());
    }
    
    private IEnumerator loadNext()
    {
        while (true)
        {
            // wait for frame to prevent multi frame downloads
            yield return null;

            if (loadQueue.Count > 0 && avatarItem == null)
            
            { 
                avatarItem = loadQueue.Dequeue();
                DownloadAvatar(avatarItem);
            }
        }
    }


    //TODO add queue to the importer
    /// <summary>
    /// Creates an <see cref="AvatarItem"/> from the provided url and <see cref="Avatar"/>; loading it into the queue to be instantiated on the next frame
    /// </summary>
    /// <param name="url"></param>
    /// <param name="avatar"></param>
    public void LoadAvatar(string url, Avatar avatar)
    {
        var avatarItem = new AvatarItem(url, avatar);
        loadQueue.Enqueue(avatarItem);
    }

    private void DownloadAvatar(AvatarItem avatarItem)
    {
        var directory = DomainManager.Instance.GetAvatarDirectory(avatarItem.FileName);
        IO.ForceFolder(directory);

        WebRequestManager.Instance.DownloadSmallItem(avatarItem.JsonURL,
            (newJson =>
            {
                var jsonPath = DomainManager.Instance.AvatarSaveLocation(avatarItem.FileName, $"{avatarItem.FileName}.json");

                if (File.Exists(jsonPath))
                {
                    var oldJson = IO.ReadFromFile(jsonPath);
                    if (oldJson != newJson)
                    {
                        IO.CleanFolderContentsFolders(directory);
                        File.WriteAllText(jsonPath, newJson);
                    }
                }
                else
                {
                    IO.CleanFolderContentsFolders(directory);
                    File.WriteAllText(jsonPath, newJson);
                }
                
                avatarItem.json = JSON.Parse(newJson);
                
                for (int i = 0; i < avatarItem.Lods.Length; i++)
                {
                    var index = i;
                    var path = DomainManager.Instance.AvatarSaveLocation(avatarItem.FileName, $"{avatarItem.FileName}-{index}{avatarItem.Extension}");
                    var lodUrl = $"{avatarItem.Uri}?meshLod={index}&quality={(Quality)index}";
                    WebRequestManager.Instance.DownloadLargeItem(lodUrl,
                        path,
                        () => { ImportModel(avatarItem,path,index); },
                        (error) =>
                        {
                            Debug.Log(error);
                            avatarItem = null;
                        });
                }
            }),
            (error) =>
            {
                Debug.LogError(error);
                avatarItem = null;
            });
    }
    
    private void ImportModel(AvatarItem avatarItem,string path,int index)
    {
        Importer.LoadFromFileAsync(path,new ImportSettings(),(gltfObject =>
        {
            avatarItem.Lods[index] = gltfObject;
            gltfObject.SetActive(false);
            avatarItem.Actions.Increment();
        }));
    }
    
    /// <summary>
    /// A container for all ReadyPlayerAvatar information required for integration with the API
    /// </summary>
    public class AvatarItem
    {
        /// <summary>
        /// The URI of the Avatar, created from the supplied url in the constructor
        /// </summary>
        public Uri Uri { get; }
        
        /// <summary>
        /// The Absolute path of the <see cref="Uri"/>, set within the constructor
        /// </summary>
        public string AbsolutePath { get; }
        
        /// <summary>
        /// The filename retrieved from the <see cref="AbsolutePath"/> using <see cref="Path"/>; set within the constructor
        /// </summary>
        public string FileName { get; }
        
        /// <summary>
        /// The file extension retrieved from the <see cref="AbsolutePath"/> using <see cref="Path"/>; set within the constructor
        /// </summary>
        public string Extension { get; }
        
        /// <summary>
        /// The url to the json file; set in the constructor replacing 'glb' with 'json'
        /// </summary>
        public string JsonURL { get; }
        
        /// <summary>
        /// The provided <see cref="Avatar"/> upon class construction
        /// </summary>
        public Avatar Avatar { get; }
        
        /// <summary>
        /// a group of empty gameObjects created within the constructor
        /// </summary>
        public GameObject[] Lods { get; }

        /// <summary>
        /// Set inside the <see cref="AvatarLoader"/> when downloading the Avatar by parsing the JSON of the avatar
        /// with <see cref="JSON"/>
        /// </summary>
        public JSONNode json;
        
        /// <summary>
        /// An action set on Object creation to set the lods of the item. 
        /// </summary>
        /// <remarks>
        /// This Action is called and incremented inside <see cref="AvatarLoader.ImportModel"/>. <see cref="TaskAction.Increment"/> is used to first increment the provided
        /// number of the task action and invoke the provided callback. 
        /// </remarks>
        public TaskAction Actions { get; }

        /// <summary>
        /// Constructor of the AvatarItem, all but the <see cref="json"/> item is set here. 
        /// </summary>
        /// <param name="url">The URL for the provided <see cref="ReadyPlayerMe"/> Avatar</param>
        /// <param name="avatar">The <see cref="Avatar"/> that's required for loading</param>
        public AvatarItem(string url,  Avatar avatar)
        {
            Avatar = avatar;
            Lods = new GameObject[3];
            Uri = new Uri(url);
            AbsolutePath = Uri.AbsolutePath;
            FileName = Path.GetFileNameWithoutExtension(AbsolutePath);
            Extension = Path.GetExtension(AbsolutePath);
            JsonURL = url.Replace("glb", "Json");
            Actions = new TaskAction(Lods.Length, () => {
                Avatar.OnAvatarLoaded(this);
                avatarItem = null;
            });
        }
    }
}