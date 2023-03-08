using System;
using UnityEngine;

/// <summary>
/// This class extends ContentModule and sets a module up to play audioclip(s)
/// </summary>
/// <see cref="ContentModule"/>
public class AudioModule : ContentModule
{
    private new AudioModuleData data;
    private AudioSource audioSource;
    private const float TOLERANCE = 0.1f;
    
    /// <summary>
    /// Loads audio clips from specified mediaguids in data
    /// Adds clips to a collection to be cycled through
    /// </summary>
    /// <param name="baseModuleData"></param>
    /// <param name="callback"></param>
    public override void Init(BaseModuleData baseModuleData, Action callback)
    {
        data = (AudioModuleData) baseModuleData;
        playlist = new AudioClip[data.mediaGuids.Count];
        
        audioSource = gameObject.ForceComponent<AudioSource>();

        AssignVariables();
        
        TaskAction task = new TaskAction(playlist.Length, () =>
        {
            UpdateContent();
            callback?.Invoke();
        });

        for (var i = 0; i < data.mediaGuids.Count; i++)
        {
            int index = i;
            var mediaGuid = data.mediaGuids[i];
            if (!string.IsNullOrEmpty(mediaGuid))
            {
                ContentManager.Instance.GetData(mediaGuid, entry =>
                {
                    var mediaEntry = (MediaEntry)entry;

                    AssetBundleLoader.Instance.LoadBundle(mediaEntry.assetData.downloadData, "", obj =>
                    {
                        if (audioSource != null)
                        {
                            playlist[index] = (AudioClip)obj;
                        }
                        else
                        {
                            Debug.LogError($"Error getting clip for {obj}");
                        }
                        
                        task.Increment();
                    });
                }, s =>
                {
                    Debug.LogError(s);
                    task.Increment();
                });
            }
        }
    }

    protected void Update()
    {
        if (audioSource != null && audioSource.clip != null && data.loop == false)
        {
            if (Math.Abs(audioSource.time - audioSource.clip.length) < TOLERANCE)
            {
                GoToNextContent();
            }
        }
    }

    private void AssignVariables()
    {
        audioSource.playOnAwake = data.playOnAwake;
        audioSource.loop = data.loop;
        audioSource.volume = data.volume;
        audioSource.spatialBlend = data.spatialBlend;
        audioSource.rolloffMode = data.rolloffMode;
        audioSource.spatialize = data.spatialise;
        audioSource.dopplerLevel = data.dopplerLevel;
        audioSource.spread = data.spread;
        audioSource.minDistance = data.minDistance;
        audioSource.maxDistance = data.maxDistance;
    }

    /// <summary>
    /// Stops audio from playing
    /// Clears data
    /// </summary>
    public override void Deinit()
    {
        audioSource.Stop();
        data = null;
    }

    protected override void UpdateContent()
    {
        audioSource.clip = (AudioClip)playlist[currentIndex];
        audioSource.Play();
    }
}

/// <summary>
/// Data which is modifiable by users
/// </summary>
/// <see cref="ContentModuleData"/>
[Serializable]
public class AudioModuleData : ContentModuleData
{
    /// <summary>
    /// The play on awake
    /// </summary>
    public bool playOnAwake;
    /// <summary>
    /// The loop
    /// </summary>
    public bool loop;
    /// <summary>
    /// The volume
    /// </summary>
    public float volume = 0.5f;
    /// <summary>
    /// The spatial blend
    /// </summary>
    public float spatialBlend = 1f;
    /// <summary>
    /// The linear
    /// </summary>
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
    /// <summary>
    /// The spatialise
    /// </summary>
    public bool spatialise;
    /// <summary>
    /// The doppler level
    /// </summary>
    public float dopplerLevel;
    /// <summary>
    /// The spread
    /// </summary>
    public float spread;
    /// <summary>
    /// The min distance
    /// </summary>
    public float minDistance;
    /// <summary>
    /// The max distance
    /// </summary>
    public float maxDistance;
}