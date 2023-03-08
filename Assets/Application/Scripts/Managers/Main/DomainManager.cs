using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Acts as a central location to retrieve API end point/data paths for relevant/required items 
/// </summary>
/// <remarks>
/// The DomainManager Singleton acts as a central accessing point to pre-defined data paths within the class.
/// The methods are designed to return strings for the associated path rather than the data itself.
/// With this in mind as further End Points to the API are defined and more features added this class is designed to receive continual updates expanding its functionality.
///
/// The Usage of the above methods is to encompass easy access to the api end points and occasional expected localised file structure, however, it is up to the method retrieving the
/// data from this class to make proper use of it.
/// <example>
/// If a returned data path doesn't presently exist on the system, but is returned from a method here, it's up to the retrieving method to handle the error and build the required
/// folder structure.
/// <code>
///     private void DownloadAvatar(AvatarItem avatarItem)
///     {
///         var directory = DomainManager.Instance.GetAvatarDirectory(avatarItem.FileName);
///         if(!Directory.Exists(directory))
///         {
///             Directory.CreateDirectory(directory);
///         }
///
///         // .. logic
///
///     }
/// </code>
/// This practice should be followed when retrieving data paths from the class, otherwise errors may arise
/// </example> 
///
/// Another common feature of this class is the <see cref="GetPlatform"/> method, returning a string for the relevant Runtime platform switching on <see cref="RuntimePlatform"/>.
/// This is used internally to provide the relevant asset bundle to the current runtime platform in <see cref="GetLocalAssetBundle"/>.
///
/// It should be noted that if you are working on an new/modifying an old feature of Bloktopia that requires the Bloktopia API or requires a local data path this class should
/// be where that path is stored. Allowing it to be centrally managed and accessed anywhere. This leaves this class OPEN for modification, however only it should be noted the
/// OPEN rule here only applies to public facing classes and the internals should not be modified unless breaking changes have caused the internals to malfunction. 
/// </remarks>
public class DomainManager : MonoSingleton<DomainManager>
{
    /// <summary>
    /// The expected file extension for all Unity Asset Bundles
    /// </summary>
    public const string BundleExtension = ".unity3d";
    
    /// <summary>
    /// retrieves the current <see cref="RuntimePlatform"/> of the application in a recognisable string format
    /// </summary>
    /// <returns>
    /// The current <see cref="RuntimePlatform"/> of the application in a recognisable string format
    /// Recognisable formats:
    ///     - "iOS"         : all apple devices
    ///     - "android"     : all android devices
    ///     - "windows"     : all windows runtimes (includes editor), and by default
    ///     - "webgl"       : WebGL runtime
    /// </returns>
    public static string GetPlatform()
    {
        return Application.platform switch
        {
            RuntimePlatform.OSXPlayer => "iOS",
            RuntimePlatform.OSXEditor => "iOS",
            RuntimePlatform.IPhonePlayer => "iOS",
            RuntimePlatform.Android => "android",
            RuntimePlatform.WindowsEditor => "windows",
            RuntimePlatform.WindowsPlayer => "windows",
            RuntimePlatform.WebGLPlayer => "webgl",
            _ => "windows"
        };
    }

    private Dictionary<string, string> domainManager = new();
    
    /// <summary>
    /// Initialises the DomainManager with a callback invoked on the successful load of the DomainManager
    /// </summary>
    /// <param name="callback">The callback invoked upon successful initialisation of the DomainManager</param>
    public void Init(Action callback)
    {
        CoroutineUtil.Instance.RunCoroutine(LoadDomainManager(callback));
    }

    private IEnumerator LoadDomainManager(Action callback)
    {
        var path = $"{Application.streamingAssetsPath}/DomainManager.json";

#if UNITY_EDITOR_OSX
        path = $"file://{path}";
#endif
        
        // using get request for local file here for android support
        var configText = UnityWebRequest.Get(path);

        yield return configText.SendWebRequest();

        while (!configText.isDone)
        {
            yield return null;
        }

        switch (configText.result)
        {
            case UnityWebRequest.Result.InProgress:
                break;
            case UnityWebRequest.Result.Success:
                if (!CoroutineUtil.IsEditorMode())
                {
                    ReadJson(configText, callback);
                }
                else
                {
                    domainManager = JsonUtil.ReadFromText<Dictionary<string, string>>(configText.downloadHandler.text);
                    callback?.Invoke();
                }
                break;
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError($"Domain manager loading error {configText.error} {configText.responseCode}");
                break;
            case UnityWebRequest.Result.DataProcessingError:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ReadJson(UnityWebRequest configText, Action callback)
    {
        JsonUtil.AsyncReadFromText<Dictionary<string, string>>(configText.downloadHandler.text,
            data =>
            {
                if (this == null)
                {
                    return;
                }
                
                domainManager = data;
                callback?.Invoke();
            });
    }

    /// <summary>
    /// returns the path of a local asset bundle of the provided guid
    /// </summary>
    /// <remarks>
    /// This method relies on the <see cref="Application.persistentDataPath"/> to retrieve the root path.
    /// </remarks>
    /// <param name="guid">The guid of the asset bundle being retrieved</param>
    /// <returns>
    /// <example>
    /// "C:\Users\User\AppData\LocalLow\Bloktopia\BloktopiaV2\AssetBundles\0a2421b691666384297dc3960daf8b63_windows.unity3d"
    /// </example>
    /// </returns>
    public string GetLocalAssetBundle(string guid)
    {
        return $"{Application.persistentDataPath}/AssetBundles/{guid}_{GetPlatform()}{BundleExtension}";
    }

    /// <summary>
    /// returns the Temporary file path used when creating new data for upload
    /// </summary>
    /// <param name="guid">The guid of the new item being created</param>
    /// <param name="type">
    /// The class type of Item being created
    /// <remarks>
    /// This should be used with the builtin nameof() function as follows
    /// <example><code>
    ///     nameof(ModuleClassName);
    /// </code></example></remarks>
    /// </param>
    /// <returns><example>
    /// "C:/Users/User/Develop/BloktopiaV2/TempData/data/MediaEntry/155ddb1e-d5a0-489d-8636-fb6b863d45ee.json"
    /// </example></returns>
    public string TempFilePath(string guid,string type)
    {
        return $"{Application.dataPath}/../TempData/data/{type}/{guid}.json";
    }
    
    /// <summary>
    /// returns the Temporary destination path of platform specific asset bundles
    /// </summary>
    /// <param name="platform">The Platform assigned to the destination path</param>
    /// <returns><example>"C:/Users/User/Develop/BloktopiaV2/TempData/IOS"</example></returns>
    public string TempDestinationPath(string platform)
    {
        return $"{Application.dataPath}/../TempData/{platform}";
    }

    /// <summary>
    /// returns the temporary destination path root folder 
    /// </summary>
    /// <returns><example>"C:/Users/User/Develop/BloktopiaV2/TempData"</example></returns>
    public string TempDataPath()
    {
        return $"{Application.dataPath}/../TempData";
    }

    /// <summary>
    /// Returns the root API endpoint
    /// </summary>
    /// <returns><example>"https://api.bloktopia.com/v1"</example></returns>
    public string GetTempRoot()
    {
        return $"{domainManager["API"]}";
    }
    
    /// <summary>
    /// Returns the temporary bundle path location, based on platform
    /// </summary>
    /// <param name="platform">platform of the correct assigned asset bundle</param>
    /// <param name="guid">guid of the asset bundle to be retrieved</param>
    /// <returns><example>"C:/Users/User/Develop/BloktopiaV2/TempData/IOS/155ddb1e-d5a0-489d-8636-fb6b863d45ee.unity3d"</example></returns>
    public string TempBundlePath(string platform, string guid)
    {
        return $"{Application.dataPath}/../TempData/{platform}/{guid}{BundleExtension}";
    }
    
    /// <summary>
    /// Returns the endpoint for user authentication
    /// </summary>
    /// <returns><example>"https://api.bloktopia.com/v1/Auth"</example></returns>
    public string GetUserAuth()
    {
        return $"{domainManager["API"]}/Auth";
    }

    /// <summary>
    /// Returns the endpoint for uploading asset bundles 
    /// </summary>
    /// <returns><example>"https://api.bloktopia.com/v1/Asset"</example></returns>
    public string UploadAssetBundles()
    {
        return $"{domainManager["API"]}/Asset";
    }
    
    /// <summary>
    /// Returns the asset bundle data of the provided asset bundle guid
    /// </summary>
    /// <param name="guid"></param>
    /// <returns><example>"https://api.bloktopia.com/v1/Asset/155ddb1e-d5a0-489d-8636-fb6b863d45ee"</example></returns>
    public string GetAssetBundleData(string guid)
    {
        return $"{domainManager["API"]}/Asset/{guid}";
    }
    
    /// <summary>
    /// Returns the base Content Data endpoint 
    /// </summary>
    /// <returns><example>"https://api.bloktopia.com/v1/ContentData"</example></returns>
    public string ContentDataEndPoint()
    {
        return $"{domainManager["API"]}/ContentData";
    }
    
    /// <summary>
    /// Returns the Content Data of the supplied guid 
    /// </summary>
    /// <returns><example>"https://api.bloktopia.com/v1/ContentData/155ddb1e-d5a0-489d-8636-fb6b863d45ee"</example></returns>
    public string GetData(string guid)
    {
        return $"{domainManager["API"]}/ContentData/{guid}";
    }
    
    /// <summary>
    /// Returns the Directory for the downloaded Avatar of the supplied guid 
    /// </summary>
    /// <param name="guid"></param>
    /// <returns><example>"C:\Users\User\AppData\LocalLow\Bloktopia\BloktopiaV2\Avatar\155ddb1e-d5a0-489d-8636-fb6b863d45ee"</example></returns>
    public string GetAvatarDirectory(string guid)
    {
        return $"{Application.persistentDataPath}/Avatar/{guid}";
    }

    /// <summary>
    /// Returns the direct location of the avatar with the provided "filename" and "guid"
    /// </summary>
    /// <param name="guid">guid used to locate the directory for the provided avatar</param>
    /// <param name="fileName">filename of the avatar to save</param>
    /// <returns><example>"C:\Users\User\AppData\LocalLow\Bloktopia\BloktopiaV2\Avatar\155ddb1e-d5a0-489d-8636-fb6b863d45ee/paddy.glb"</example></returns>
    public string AvatarSaveLocation(string guid,string fileName)
    {
        return $"{Application.persistentDataPath}/Avatar/{guid}/{fileName}";
    }
}
