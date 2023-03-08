using System;
using UnityEngine;
using Utilities;

/// <summary>
/// This class extends ContentModule and sets a module up to cycle through textures
/// </summary>
/// <see cref="ContentModule"/>
public class TextureModule : ContentModule
{
    private new TextureModuleData data;
    protected float time;
    protected new Renderer renderer;
    private bool isInitialised = false;
    
    /// <summary>
    /// Loads textures from specified mediaguids in data
    /// Adds textures to a collection to be cycled through
    /// </summary>
    /// <param name="baseModuleData"></param>
    /// <param name="callback"></param>
    public override void Init(BaseModuleData baseModuleData, Action callback)
    {
        data = (TextureModuleData)baseModuleData;
        playlist = new Texture[data.mediaGuids.Count];
        
        TaskAction task = new TaskAction(playlist.Length, () =>
        {
            renderer.material = Resources.Load<Material>("DecalAdvance");
            renderer.material.SetTextureOffset(data.textureRef, data.offset);
            renderer.material.SetTextureScale(data.textureRef, data.tiling);
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
                AssetBundleLoader.Instance.LoadBundle(mediaEntry.assetData.downloadData,"",
                    obj =>
                    {
                        if (obj == null)
                        {
                            Debug.LogError($"Asset bundle for guid {mediaEntry.guid} was null");
                            task.Increment();
                            return;
                        }

                        playlist[index] = (Texture)obj;

                        task.Increment();
                    });
            }, s =>
            {
                Debug.LogError($"Media entry on module {data.mediaGuids} was null");
                task.Increment();
            });
        }
        
        renderer = GetRenderer(data.rendererName);
    }

    /// <summary>
    /// Sets loop timer to 0
    /// </summary>
    public override void Deinit()
    {
        time = 0;
        base.Deinit();
    }

    protected override void UpdateContent()
    {
        if (renderer != null)
        {
            ZoomUtil.SetWidth(renderer.material, gameObject, (Texture)playlist[currentIndex], "_DecalTiling", "_DecalOffset");
            renderer.material.SetTexture(data.textureRef, (Texture)playlist[currentIndex]);
        }
    }
    
    protected void Update()
    {
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

    private Renderer GetRenderer(string surface)
    {
        if (string.IsNullOrEmpty(surface))
        {
            return GetComponent<Renderer>();
        }
        else
        {
            return transform.Find(surface).GetComponent<Renderer>();
        }
    }
}

/// <summary>
/// Data which is modifiable by users
/// </summary>
/// <see cref="ContentModuleData"/>
[Serializable]
public class TextureModuleData : ContentModuleData
{
    /// <summary>
    /// The swap time
    /// </summary>
    public float swapTime;
    /// <summary>
    /// The renderer name
    /// </summary>
    public string rendererName;
    /// <summary>
    /// The texture ref
    /// </summary>
    public string textureRef;
    /// <summary>
    /// The one
    /// </summary>
    public Vector2 tiling = Vector2.one;
    /// <summary>
    /// The zero
    /// </summary>
    public Vector2 offset = Vector2.zero;
}