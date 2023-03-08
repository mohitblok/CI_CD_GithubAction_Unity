using System;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

/// <summary>
/// This class extends ContentModule and sets a module up to cycle through videos
/// </summary>
/// <see cref="ContentModule"/>
public class VideoModule : ContentModule
{
    private const double TOLERANCE = 1f;

    private new VideoModuleData data;
    private MediaPlayer mediaPlayer;
    private Material lastMaterial;
    private AudioSource audioSource;

    /// <summary>
    /// Loads video urls from specified mediaguids in data
    /// Adds video urls to a collection to be cycled through
    /// </summary>
    /// <param name="baseModuleData"></param>
    /// <param name="callback"></param>
    public override void Init(BaseModuleData baseModuleData, Action callback)
    {
        data = (VideoModuleData)baseModuleData;
        playlist = new string[data.mediaGuids.Count];
        TaskAction task = new TaskAction(playlist.Length, () =>
        {
            UpdateContent();
            callback?.Invoke();
        });
        
        InitMediaPlayer();

        for (var i = 0; i < data.mediaGuids.Count; i++)
        {
            var index = i;
            var mediaGuid = data.mediaGuids[i];
            ContentManager.Instance.GetData(mediaGuid, entry =>
            {
                var mediaEntry = (MediaEntry)entry;
                playlist[index] = mediaEntry.assetData.downloadData;
                task.Increment();
            }, s =>
            {
                Debug.LogError($"Media entry on module {data.mediaGuids} was null");
                task.Increment();
            });
        }
    }

    /// <summary>
    /// Closes media players
    /// Stops videos from playing
    /// </summary>
    public override void Deinit()
    {
        base.Deinit();
        mediaPlayer.CloseMedia();
        mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
        gameObject.GetComponent<Renderer>().material = lastMaterial;
    }

    private void InitMediaPlayer()
    {
        mediaPlayer = gameObject.ForceComponent<MediaPlayer>();
        audioSource = gameObject.ForceComponent<AudioSource>();
        var audioOutput = gameObject.ForceComponent<AudioOutput>();
        var applyToMaterial = gameObject.ForceComponent<ApplyToMaterial>();
        
        AssignVariables();

        mediaPlayer.PlatformOptionsWindows.audioOutput = Windows.AudioOutput.Unity;
        mediaPlayer.PlatformOptionsAndroid.audioOutput = Android.AudioOutput.Unity;
        mediaPlayer.PlatformOptionsIOS.audioMode = MediaPlayer.OptionsApple.AudioMode.Unity;

        audioOutput.Player = mediaPlayer;

        applyToMaterial.Material = Resources.Load<Material>("AVProVideo");
        applyToMaterial.Player = mediaPlayer;
        var renderer = gameObject.GetComponent<Renderer>();
        lastMaterial = renderer.material;
        renderer.material = applyToMaterial.Material;

        mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
    }

    private void OnMediaPlayerEvent(MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
    {
        switch (eventType)
        {
            case MediaPlayerEvent.EventType.FinishedPlaying:
                if (!data.loop)
                {
                    if (Math.Abs(this.mediaPlayer.Info.GetDuration() - this.mediaPlayer.Control.GetCurrentTime()) <
                        TOLERANCE)
                    {
                        GoToNextContent();
                    }
                }

                break;
        }
    }

    protected override void UpdateContent()
    {
        mediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, (string)playlist[currentIndex]);
    }
    
    private void AssignVariables()
    {
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
    
}

/// <summary>
/// Data which is modifiable by users
/// </summary>
/// <see cref="ContentModuleData"/>
[Serializable]
public class VideoModuleData : ContentModuleData
{
    /// <summary>
    /// The loop
    /// </summary>
    public bool loop = false;
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
    
    /// <summary>
    /// This virtual method is used to retrieve a list of string dependencies for a module.
    /// It returns an empty list by default and can be overridden in derived classes to provide module-specific dependency data.
    /// Dependency data is data that needs to be downloaded for a specific module to function correctly.
    /// </summary>
    /// <returns>Type of list<string></returns>
    public override List<string> GetDependencyData()
    {
        return new List<string>();
    }
}