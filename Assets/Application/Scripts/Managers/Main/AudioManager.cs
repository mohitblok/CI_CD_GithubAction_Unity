using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class AudioManager : MonoSingleton<AudioManager>
{
    public enum AudioChannel
    {
        Environment,
        Speech,
        Music,
    }

    [field: SerializeField] public bool environmentVolume { get; private set; }
    [field: SerializeField] public bool speechVolume { get; private set; }
    [field: SerializeField] public bool musicVolume { get; private set; }

    public static void SetAudioChannelVolume(AudioChannel audioChannel, float volume) { }
}
