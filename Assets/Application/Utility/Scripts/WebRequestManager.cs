using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Manages all web requests
/// </summary>
public class WebRequestManager : MonoSingleton<WebRequestManager>
{
    /// <summary>
    /// The method to use when uploading files
    /// </summary>
    public enum UploadMethod
    {
        POST,
        PUT,
    }

    private const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIweGM3Q2I5NTJFZDMwRTA1ZDViYmRFQkFFMTk3MzhjMjE5NEZkY2U3OUMiLCJyb2xlIjpbIkFkbWluaXN0cmF0b3IiXSwiaWQiOiI0OTI1MjRkZi1hMWMwLTQ0MDAtYjlmZC1jMTgwMGVmMjA1OTMiLCJuYmYiOjE2NzY2NDA0MzMsImV4cCI6MTY3NjY3NjQzMywiaWF0IjoxNjc2NjQwNDMzLCJpc3MiOiJhcGkuYmxva3RvcGlhLmNvbSIsImF1ZCI6IkJsb2t0b3BpYW5zIn0.eN3dXQx6ZWuOdw8ufz4LpKy740kyp3tG-XpKWBbdve0";

    private const string CONTENT_TYPE_JSON = "application/json";
    private const string CONTENT_TYPE_BUNDLE = "application/octet-stream";

    private Queue<WebRequestItem> largeItemQueue = new Queue<WebRequestItem>();
    private Queue<WebRequestItem> smallItemQueue = new Queue<WebRequestItem>();
    private Dictionary<string, DownloadItem> lookup = new Dictionary<string, DownloadItem>();

    private const int MAX_RETRY_COUNT = 2;
    private const int TIMEOUT = 5;

    private void Start()
    {
        CoroutineUtil.Instance.RunCoroutine(RunLargeItemQueue());
        CoroutineUtil.Instance.RunCoroutine(RunSmallItemQueue());
    }

#if UNITY_EDITOR
    /// <summary> The WakeUp function is called by the game when the object becomes active and starts running.
    /// This function will be called every time a MonoBehaviour is enabled.</summary>
    public void WakeUp()
    {
        CoroutineUtil.Instance.RunCoroutine(RunLargeItemQueue());
        CoroutineUtil.Instance.RunCoroutine(RunSmallItemQueue());
    }
#endif

    /// <summary> The UploadLargeItem function is used to upload large files.
    /// <param name="url"> The url to upload the item to.</param>
    /// <param name="method"> /// the method to use for uploading the file.
    /// </param>
    /// <param name="uploadHandler"> /// the UploadHandler is a delegate that will be called when the upload finishes.  it takes two parameters:
    /// 1) a string containing the url of the uploaded file, and 2) an action to call if there was an error uploading.
    /// </param>
    /// <param name="successCallback"> The callback to be called when the upload succeeds.</param>
    /// <param name="errorCallback"> The callback to be called when the upload errors.</param>
    public void UploadLargeItem(string url, UploadMethod method, UploadHandler uploadHandler, Action successCallback, Action<string> errorCallback)
    {
        UploadItem item = new UploadItem(url, method, uploadHandler, successCallback, errorCallback);
        largeItemQueue.Enqueue(item);
    }


    /// <summary> The UploadSmallItem function is used to upload a small item (less than 1 MB) to the server.
    /// It takes in a url, method, and data object as parameters. The url is the address of where you want
    /// your data uploaded to on the server. The method parameter can be set either &quot;POST&quot; or &quot;PUT&quot;. If it's POST, then 
    /// all of your data will be sent with one request; if it's PUT then each field will have its own request.</summary>
    /// <param name="url"> The url to upload the item to.</param>
    /// <param name="method"> /// the method to use for uploading the file.</param>
    /// <param name="successCallback"> The callback to be called when the upload succeeds.</param>
    /// <param name="errorCallback"> The callback to be called when the upload errors.</param>
    public void UploadSmallItem(string url, UploadMethod method, ContentDataEntry data, Action successCallback, Action<string> errorCallback)
    {
        var json = JsonUtil.WriteToText(data, false);
        var postData = Encoding.UTF8.GetBytes(json);
        UploadHandler uploader = new UploadHandlerRaw(postData);
        uploader.contentType = CONTENT_TYPE_JSON;
        UploadItem item = new UploadItem(url, method, uploader, successCallback, errorCallback);
        smallItemQueue.Enqueue(item);
    }

    /// <summary>
    /// download a large item
    /// </summary>
    /// <param name="url">the url to upload to</param>
    /// <param name="path">path to item</param>
    /// <param name="successCallback">callback on success</param>
    /// <param name="errorCallback">callback on error</param>
    public void DownloadLargeItem(string url, string path, Action successCallback, Action<string> errorCallback)
    {
        if (lookup.ContainsKey(url))
        {
            DownloadItem item = lookup[url];
            item.SubSuccessCallback(successCallback);
            item.SubErrorCallback(errorCallback);
        }
        else
        {
            DownloadItem item = new DownloadItem(url, path, successCallback, errorCallback);
            largeItemQueue.Enqueue(item);
            lookup[url] = item;
        }
    }

    /// <summary>
    /// download a small item
    /// </summary>
    /// <param name="url">the url to download to</param>
    /// <param name="successCallback">callback on success</param>
    /// <param name="errorCallback">callback on error</param>
    public void DownloadSmallItem<T>(string url, Action<T> successCallback, Action<string> errorCallback) where T : class
    {
        DownloadDataItem item = new DownloadDataItem(url,
            (s) =>
            {
                if (!CoroutineUtil.IsEditorMode())
                {
                    JsonUtil.AsyncReadFromText<T>(s,
                        data =>
                        {
                            if (this)
                            {
                                if (data == null)
                                {
                                    errorCallback?.Invoke("Data was null");
                                    return;
                                }

                                successCallback?.Invoke(data);
                            }
                        });
                }
                else
                {
                    Debug.Log(s);

                    var data = JsonUtil.ReadFromText<T>(s);
                    successCallback?.Invoke(data);
                }
            },
            errorCallback);
        smallItemQueue.Enqueue(item);
    }

    /// <summary>
    /// download a small item
    /// </summary>
    /// <param name="url">the url to download to</param>
    /// <param name="successCallback">callback on success</param>
    /// <param name="errorCallback">callback on error</param>
    public void DownloadSmallItem(string url, Action<string> successCallback, Action<string> errorCallback)
    {
        DownloadDataItem item = new DownloadDataItem(url, successCallback, errorCallback);
        smallItemQueue.Enqueue(item);
    }

    private IEnumerator RunLargeItemQueue()
    {
        while (true)
        {
            if (largeItemQueue.Count > 0)
            {
                WebRequestItem item = largeItemQueue.Dequeue();

                if (item is DownloadItem downloadItem)
                {
                    if (File.Exists(downloadItem.Path))
                    {
                        item.SuccessCallback?.Invoke();
                    }
                    else
                    {
                        yield return Download(downloadItem);
                    }

                    RemoveLookup(downloadItem);
                }
                else if (item is UploadItem uploadItem)
                {
                    yield return Upload(uploadItem);
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator RunSmallItemQueue()
    {
        while (true)
        {
            if (smallItemQueue.Count > 0)
            {
                WebRequestItem item = smallItemQueue.Dequeue();

                if (item is DownloadItem downloadItem)
                {
                    yield return Download(downloadItem);
                }
                else if (item is DownloadDataItem downloadDataItem)
                {
                    yield return DownloadData(downloadDataItem);
                }
                else if (item is UploadItem uploadItem)
                {
                    yield return Upload(uploadItem);
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator Download(DownloadItem item)
    {
        int retries = 0;
        bool success = false;

        while (!success && retries <= MAX_RETRY_COUNT)
        {
            var request = UnityWebRequest.Get(item.Url);
            //TODO doesnt work for large files need to check for progress and timeout if progress hasnt changed for TIMEOUT seconds
            //request.timeout = TIMEOUT;
            request.downloadHandler = new DownloadHandlerFile(item.Path);
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                retries++;
                Debug.LogError("Download failed: " + request.error);
                item.ErrorCallback?.Invoke(request.error);
            }
            else
            {
                success = true;
                item.SuccessCallback?.Invoke();
            }
        }
    }

    private IEnumerator DownloadData(DownloadDataItem item)
    {
        int retries = 0;
        bool success = false;

        while (!success && retries <= MAX_RETRY_COUNT)
        {
            using (var request = UnityWebRequest.Get(item.Url))
            {
                request.timeout = TIMEOUT;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    success = true;
                    item.SuccessCallback?.Invoke(request.downloadHandler.text);
                }
                else
                {
                    retries++;
                    Debug.LogError($"Download failed at url: {request.url} - error: {request.error} downloadHandler: {request.downloadHandler.text}");
                    item.ErrorCallback?.Invoke(request.error);
                }
            }
        }
    }

    private IEnumerator Upload(UploadItem item)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(item.Url))
        {
            //TODO doesnt work for large files need to check for progress and timeout if progress hasnt changed for TIMEOUT seconds
            //webRequest.timeout = TIMEOUT;
            webRequest.method = item.Method.ToString();
            webRequest.uploadHandler = item.UploadHandler;
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
            webRequest.SendWebRequest();

            while (!webRequest.isDone)
            {
                //progressCallback?.Invoke(webRequest.uploadProgress);
                yield return null;
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                item.SuccessCallback?.Invoke();
            }
            else
            {
                item.ErrorCallback?.Invoke($"{webRequest.error} | {webRequest.downloadHandler.text}");
            }
        }
    }

    private void RemoveLookup(DownloadItem item)
    {
        if (lookup.ContainsKey(item.Url))
            lookup.Remove(item.Url);
    }

    /// <summary> The CreateMultiPartFormDataSection function creates a list of IMultipartFormSection objects that can be used to upload a bundle file.</summary>
    /// <param name="guid"> The guid of the bundle to upload.</param>
    /// <param name="bundlePath"> The path to the bundle file.</param>
    /// <param name="platform"> The platform of the bundle.</param>
    /// <returns> A list of IMultipartFormSection objects.</returns>
    public List<IMultipartFormSection> CreateMultiPartFormDataSection(string guid, string bundlePath, string platform)
    {
        List<IMultipartFormSection> form = new List<IMultipartFormSection> { new MultipartFormDataSection("DownloadDataId", guid), new MultipartFormFileSection("AssetFile", File.ReadAllBytes($"{bundlePath}"), $"{guid}_{platform}{DomainManager.BundleExtension}", CONTENT_TYPE_BUNDLE), };

        return form;
    }

    private abstract class WebRequestItem
    {
        /// <summary>
        /// url to download from
        /// </summary>
        public string Url { get; protected set; }

        /// <summary>
        /// callback to call on success
        /// </summary>
        public Action SuccessCallback { get; protected set; }

        /// <summary>
        /// callback to call on error
        /// </summary>
        public Action<string> ErrorCallback { get; protected set; }
    }

    private class DownloadItem : WebRequestItem
    {
        /// <summary>
        /// path to download to
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// subscribe to success callback
        /// </summary>
        /// <param name="successCallback"></param>
        public void SubSuccessCallback(Action successCallback)
        {
            SuccessCallback += successCallback;
        }

        /// <summary>
        /// subscribe to error callback
        /// </summary>
        /// <param name="errorCallback"></param>
        public void SubErrorCallback(Action<string> errorCallback)
        {
            ErrorCallback += errorCallback;
        }

        /// <summary>
        /// DownloadItem constructor
        /// </summary>
        /// <param name="url">url to download from</param>
        /// <param name="path">path to save to</param>
        /// <param name="successCallback">callback on success</param>
        /// <param name="errorCallback">callback on error</param>
        public DownloadItem(string url, string path, Action successCallback, Action<string> errorCallback)
        {
            Url = url;
            Path = path;
            SuccessCallback = successCallback;
            ErrorCallback = errorCallback;
        }
    }

    private class UploadItem : WebRequestItem
    {
        /// <summary>
        /// method to use for upload
        /// </summary>
        public UploadMethod Method { get; protected set; }
        
        /// <summary>
        /// upload handler
        /// </summary>
        public UploadHandler UploadHandler { get; private set; }

        /// <summary>
        /// Construct an UploadItem
        /// </summary>
        /// <param name="url">url to upload to</param>
        /// <param name="method">upload method</param>
        /// <param name="uploadHandler">upload handler</param>
        /// <param name="successCallback">callback on success</param>
        /// <param name="errorCallback">callback on error</param>
        public UploadItem(string url, UploadMethod method, UploadHandler uploadHandler, Action successCallback, Action<string> errorCallback)
        {
            Url = url;
            Method = method;
            UploadHandler = uploadHandler;
            SuccessCallback = successCallback;
            ErrorCallback = errorCallback;
        }
    }

    private class DownloadDataItem : WebRequestItem
    {
        /// <summary>
        /// callback to call on success
        /// </summary>
        public new Action<string> SuccessCallback { get; protected set; }

        /// <summary>
        /// download data item constructor
        /// </summary>
        /// <param name="url">url to download to</param>
        /// <param name="successCallback">callback on success</param>
        /// <param name="errorCallback">callback on erro</param>
        public DownloadDataItem(string url, Action<string> successCallback, Action<string> errorCallback)
        {
            Url = url;
            SuccessCallback = successCallback;
            ErrorCallback = errorCallback;
        }
    }
}