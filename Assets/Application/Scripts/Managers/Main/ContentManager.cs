using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Manager for all content data entries </summary>
/// <see cref="ContentDataEntry"/> is the base class for all content data entries <see cref="ContentDataEntry"/>
public class ContentManager : MonoSingleton<ContentManager>
{
    private Dictionary<string, ContentDataEntry> dictOfDataEntries = new Dictionary<string, ContentDataEntry>();
    private Dictionary<Type, List<string>> dictOfTypeToGuid = new Dictionary<Type, List<string>>();

#if UNITY_EDITOR
    public Dictionary<string, ContentDataEntry> DictOfDataEntries => dictOfDataEntries;
#endif

    /// <summary>
    /// The GetData function retrieves the data from the server and adds it to the local database.
    /// </summary>
    /// <param name="callback">returns the list of ContentDataEntry objects if successful</param>
    /// <param name="types">the types of data to be retrieved. if null, all data will be returned.</param>
    /// <returns> A list of ContentDataEntry objects.</returns>
    public void GetData(Action<List<ContentDataEntry>> callback, List<Type> types = null)
    {
        var url = DomainManager.Instance.ContentDataEndPoint();

        if (types != null)
        {
            url += "?contentDataTypes=";

            for (var index = 0; index < types.Count; index++)
            {
                var type = types[index];

                if (type.IsAssignableFrom(typeof(ContentDataEntry)))
                {
                    Debug.LogError("This is only going to work with a type of content");
                    return;
                }

                url += index == types.Count - 1 ? $"{type}" : $"{type},";
            }
        }

        WebRequestManager.Instance.DownloadSmallItem<List<ContentDataEntry>>(url,
            list =>
            {
                for (var index = 0; index < list.Count; index++)
                {
                    var file = list[index];
                    AddDataEntry(file);
                }

                callback.Invoke(list);
            },
            s => { Debug.LogError($"data is null for {s}"); });
    }
    
    /// <summary> The ClearData function clears the dictionary of data entries.</summary>
    public void ClearData()
    {
        dictOfDataEntries.Clear();
    }

    /// <summary> The GetData function retrieves the data for a given guid from the server and returns it to the caller.</summary>
    /// <param name="guid"> The guid of the data entry to be retrieved.</param>
    /// <param name="onSuccess"> /// this is the callback function that will be called when the data has been downloaded.</param>
    /// <param name="onError"> The action to call when the download fails.</param>
    /// <returns> A ContentDataEntry object.</returns>
    public void GetData(string guid, Action<ContentDataEntry> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(guid))
        {
            onError?.Invoke("guid is null");
            return;
        }

        if (dictOfDataEntries.ContainsKey(guid))
        {
            onSuccess?.Invoke(dictOfDataEntries[guid]);
        }
        else
        {
            WebRequestManager.Instance.DownloadSmallItem<ContentDataEntry>(DomainManager.Instance.GetData(guid),
                (data) =>
                {
                    dictOfDataEntries[guid] = data;
                    onSuccess?.Invoke(dictOfDataEntries[guid]);
                },
                onError);
        }
    }

    public void AddDataEntry(ContentDataEntry data)
    {
        dictOfDataEntries[data.guid] = data;
        var source = dictOfDataEntries[data.guid];
        var type = data.GetType();

        if (false == dictOfTypeToGuid.ContainsKey(type))
        {
            dictOfTypeToGuid.Add(type, new List<string>());
        }

        if (false == dictOfTypeToGuid[type].Contains(source.guid))
        {
            dictOfTypeToGuid[type].Add(source.guid);
        }

        if (source is MediaEntry)
        {
            Type mediaType = typeof(MediaEntry);
            if (false == dictOfTypeToGuid.ContainsKey(mediaType))
            {
                dictOfTypeToGuid.Add(mediaType, new List<string>());
            }

            if (false == dictOfTypeToGuid[mediaType].Contains(source.guid))
            {
                dictOfTypeToGuid[mediaType].Add(source.guid);
            }
        }
    }
}