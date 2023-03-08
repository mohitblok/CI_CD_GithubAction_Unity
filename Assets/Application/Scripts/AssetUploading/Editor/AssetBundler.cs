using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Utilities;

/// <summary>
/// asset bundler Editor Window used to bundle assets
/// </summary>
public class AssetBundler : EditorWindow
{
    private static AssetBundler window;
    private static MediaEntry currentMediaEntry;

    private Queue<Action> uploadQueue = new Queue<Action>();
    private int activeRequests = 0;
    private const int MAX_ACTIVE_REQUESTS = 1;

    /// <summary> The ShowAssetCreator function is used to create a window that allows the user to select an asset bundle and then add it to the current media entry.</summary>
    /// <param name="mediaEntry"> The mediaentry to be bundled</param>
    public static void ShowAssetCreator(MediaEntry mediaEntry)
    {
        window = GetWindow<AssetBundler>(false, "Asset Bundler", true);
        window.Show();
        currentMediaEntry = mediaEntry;
    }

    private void OnEnable()
    {
        CoroutineUtil.CreateInstance();
        MainThreadDispatcher.CreateInstance();
        DomainManager.CreateInstance();
        ContentManager.CreateInstance();
        DomainManager.Instance.Init(() => { WebRequestManager.CreateInstance(); });
    }

    private void OnGUI()
    {
        if (currentMediaEntry != null)
        {
            EditorGUILayout.LabelField(currentMediaEntry.guid);
            EditorGUILayout.LabelField(currentMediaEntry.name);
            currentMediaEntry.extension = EditorGUILayout.TextField("Extension", currentMediaEntry.extension);
            currentMediaEntry.description = EditorGUILayout.TextField("Description", currentMediaEntry.description);
            currentMediaEntry.media = (Media)EditorGUILayout.EnumPopup("MediaType", currentMediaEntry.media);

            if (GUILayout.Button("Create"))
            {
                if (AssetCreationChecks() == false) return;

                PreBundleStep();
                BundleStep();
                AssetDatabase.Refresh();
            }
        }
        else
        {
            EditorGUILayout.LabelField("Media entry is null");
        }
    }

    private void PreBundleStep()
    {
        var bundleNames = AssetDatabase.GetAllAssetBundleNames();

        foreach (var bundleName in bundleNames)
        {
            AssetDatabase.RemoveAssetBundleName(bundleName, true);
        }
    }

    private bool AssetCreationChecks()
    {
        if (Selection.count == 0)
        {
            Debug.Log("Please select an asset in the project and try again");
            return false;
        }

        return true;
    }

    private void BundleStep()
    {
        var assetPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

        var assetName = Selection.assetGUIDs[0];

        AssetImporter.GetAtPath(assetPath).assetBundleName = $"{assetName}{DomainManager.BundleExtension}";

        currentMediaEntry.originalSource = assetPath;

        CreateAssetBundle("windows", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        CreateAssetBundle("android", BuildAssetBundleOptions.None, BuildTarget.Android);
        CreateAssetBundle("iOS", BuildAssetBundleOptions.None, BuildTarget.iOS);
        CreateAssetData(assetName);
    }

    private void CreateAssetBundle(string platform, BuildAssetBundleOptions options, BuildTarget buildTarget)
    {
        CreateRootFolder();
        CreateSubFolder("windows");
        CreateSubFolder("iOS");
        CreateSubFolder("android");

        BuildPipeline.BuildAssetBundles(
            DomainManager.Instance.TempDestinationPath(platform),
            options,
            buildTarget);
    }

    private void CreateAssetData(string guid)
    {
        currentMediaEntry.creation = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
        currentMediaEntry.assetData.downloadData = guid;

        currentMediaEntry.assetData.assetHashDict["windows"] =
            HashChecker.GetHash(DomainManager.Instance.TempBundlePath("windows", guid));
        currentMediaEntry.assetData.assetHashDict["android"] =
            HashChecker.GetHash(DomainManager.Instance.TempBundlePath("android", guid));
        currentMediaEntry.assetData.assetHashDict["iOS"] =
            HashChecker.GetHash(DomainManager.Instance.TempBundlePath("iOS", guid));
        JsonUtil.WriteToFile<ContentDataEntry>(currentMediaEntry,
            DomainManager.Instance.TempFilePath(currentMediaEntry.guid, nameof(MediaEntry)));

        ContentManager.Instance.GetData(list =>
        {
            WebRequestManager.Instance.WakeUp();

            var task = new TaskAction(4, CleanFolders);
            
            UploadBundles(guid, DomainManager.Instance.TempBundlePath("android", guid), "android", task.Increment);
            UploadBundles(guid, DomainManager.Instance.TempBundlePath("iOS", guid), "ios", task.Increment);
            UploadBundles(guid, DomainManager.Instance.TempBundlePath("windows", guid), "windows", task.Increment);
            
            var didPost = false;

            foreach (var entry in list)
            {
                if (entry is MediaEntry mediaEntry)
                {
                    if (mediaEntry.guid == currentMediaEntry.guid)
                    {
                        currentMediaEntry.guid = mediaEntry.guid;
                        UploadData(currentMediaEntry, WebRequestManager.UploadMethod.PUT, task.Increment);
                        didPost = true;
                        break;
                    }
                }
            }

            if (!didPost)
            {
                UploadData(currentMediaEntry, WebRequestManager.UploadMethod.POST, task.Increment);
            }
        });
    }

    private void UploadBundles(string guid, string bundlePath, string platform, Action callback)
    {
        var boundary = UnityWebRequest.GenerateBoundary();
        var multiPartFormData =
            UnityWebRequest.SerializeFormSections(
                WebRequestManager.Instance.CreateMultiPartFormDataSection(guid, bundlePath, platform), boundary);
        var contentType = string.Concat("multipart/form-data; boundary=", Encoding.UTF8.GetString(boundary));

        UploadHandler uploader = new UploadHandlerRaw(multiPartFormData);
        uploader.contentType = contentType;

        WebRequestManager.Instance.UploadLargeItem(DomainManager.Instance.UploadAssetBundles(),
            WebRequestManager.UploadMethod.POST,
            uploader,
            () =>
            {
                Debug.Log("Asset bundle upload success");
                callback?.Invoke();
            },
            s =>
            {
                Debug.LogError($"Asset bundle upload error - {s}");
            });
    }

    private void UploadData(ContentDataEntry data, WebRequestManager.UploadMethod method, Action callback)
    {
        WebRequestManager.Instance.UploadSmallItem(DomainManager.Instance.ContentDataEndPoint(), method,
            data,
            () =>
            {
                Debug.Log("Data upload success");
                callback?.Invoke();
            }, s =>
            {
                Debug.LogError($"Data upload error - {s}");
            });
    }

    private void CreateRootFolder()
    {
        IO.ForceFolder(DomainManager.Instance.TempDataPath());
    }

    private void CreateSubFolder(string platform)
    {
        IO.ForceFolder($"{DomainManager.Instance.TempDataPath()}/{platform}");
    }

    private void CleanFolders()
    {
        IO.CleanFolderContentsFolders(DomainManager.Instance.TempDataPath());

        if (Directory.Exists(DomainManager.Instance.TempDataPath()))
        {
            Directory.Delete(DomainManager.Instance.TempDataPath());
        }
    }
}