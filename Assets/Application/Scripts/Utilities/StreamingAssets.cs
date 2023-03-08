using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Utilities
{
    public static class StreamingAssets
    {
        public delegate void LoadCallback(string error, string content);

        public static string ToAbsolutePath(string relativePath)
        {
            return Path.Combine(Application.streamingAssetsPath, relativePath);
        }

        public static IEnumerator LoadText(string relativePath, LoadCallback callback)
        {
            string absolutePath = ToAbsolutePath(relativePath);
            using (UnityWebRequest request = UnityWebRequest.Get(absolutePath))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                    callback?.Invoke(request.error ?? "Unknown error", null);
                else
                    callback?.Invoke(null, request.downloadHandler.text);
            }
        }
    }
}