using System;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// This class extends ContentModule and sets a module up to cycle through models
/// </summary>
/// <see cref="ContentModule"/>
public class ModelModule : ContentModule
{
    private new ModelModuleData data;
    protected float time;

    private GameObject currentModel;
    private Bounds boundingBox;
    private Quaternion rotation;
    private bool isInitialised = false;
    
    /// <summary>
    /// Loads models from specified mediaguids in data
    /// Adds models to a collection to be cycled through
    /// </summary>
    /// <param name="baseModuleData"></param>
    /// <param name="callback"></param>
    public override void Init(BaseModuleData baseModuleData, Action callback)
    {
        data = (ModelModuleData)baseModuleData;
        playlist = new GameObject[data.mediaGuids.Count];
        TaskAction task = new TaskAction(playlist.Length, () =>
        {
            isInitialised = true;
            UpdateContent();
            callback?.Invoke();
        });

        for (var i = 0; i < data.mediaGuids.Count; i++)
        {
            int index = i;
            var guid = data.mediaGuids[i];
            ContentManager.Instance.GetData(guid, entry =>
            {
                var mediaEntry = (MediaEntry)entry;

                AssetBundleLoader.Instance.LoadBundle(mediaEntry.assetData.downloadData, "", obj =>
                {
                    if (obj == null)
                    {
                        Debug.LogError($"Asset bundle for guid {mediaEntry.guid} was null");
                        task.Increment();
                        return;
                    }

                    playlist[index] = Instantiate((GameObject)obj, transform);
                    ((GameObject)playlist[index]).SetActive(false);
                    var modelSize = ((GameObject)playlist[index]).CalculateRendererBounds().size;
                    var scale = modelSize.magnitude / boundingBox.size.magnitude;
                    ((GameObject)playlist[index]).transform.localScale = Vector3.one * (1 / scale);
                    ((GameObject)playlist[index]).transform.localPosition += data.boundsPosition;
                    task.Increment();
                });
            }, s =>
            {
                Debug.LogError($"Media entry on module {guid} was null");
                task.Increment();
            });
        }

        boundingBox = new Bounds(data.boundsCenter, data.boundsSize);
    }
    
    protected void Update()
    {
        if (currentModel != null)
        {
            rotation.eulerAngles += data.rotationSpeed * Time.deltaTime;
            currentModel.transform.rotation = rotation;
        }
        
        if (CanUpdate())
        {
            time += Time.deltaTime;

            if (time >= data.swapTime)
            {
                GoToNextContent();
                time = 0f;
            }
        }
    }
    
    private bool CanUpdate()
    {
        return playlist != null && playlist.Length > 1 && data.swapTime > 0 && isInitialised;
    }

    /// <summary>
    /// Deletes all models in collection
    /// Clears data
    /// </summary>
    public override void Deinit()
    {
        foreach (var model in playlist)
        {
            Destroy((GameObject)model);   
        }
        rotation = quaternion.identity;
        time = 0;
        base.Deinit();
    }

    protected override void UpdateContent()
    {
        if (currentModel != null)
        {
            currentModel.SetActive(false);
        }

        currentModel = (GameObject)playlist[currentIndex];
        
        if (currentModel != null)
        {
            currentModel.SetActive(true);
        }
    }
}

/// <summary>
/// Data which is modifiable by users
/// </summary>
/// <see cref="ContentModuleData"/>
[Serializable]
public class ModelModuleData : ContentModuleData
{
    /// <summary>
    /// The swap time
    /// </summary>
    public float swapTime;
    /// <summary>
    /// The one
    /// </summary>
    public Vector3 boundsSize = Vector3.one;
    /// <summary>
    /// The zero
    /// </summary>
    public Vector3 boundsCenter = Vector3.zero;
    /// <summary>
    /// The one
    /// </summary>
    public Vector3 boundsPosition = Vector3.one;
    /// <summary>
    /// The zero
    /// </summary>
    public Vector3 rotationSpeed = Vector3.zero;
}