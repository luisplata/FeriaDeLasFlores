using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Audio manager for the game
/// </summary>
public static class AudioManager
{
    private static string SoundsFolder = $"Sounds{Path.DirectorySeparatorChar}";

    private static bool initialized = false;
    private static AudioSource audioSource;
    private static Dictionary<AudioClipName, AudioClip> audioClips = new Dictionary<AudioClipName, AudioClip>();


    /// <summary>
    /// Gets wheter or not the audio manager has been initialized
    /// </summary>
    public static bool Initialized
    {
        get { return initialized;  }
    }
    /// <summary>
    /// Initialzies the audio manager
    /// </summary>
    /// <param name="source">audio source</param>
    public static void Initialize(AudioSource source)
    {
        initialized = true;
        audioSource = source;

        foreach (AudioClip clip in Resources.LoadAll<AudioClip>(SoundsFolder))
        {
            int enumClip = (int)Enum.Parse(typeof(AudioClipName), clip.name);
            audioClips.Add((AudioClipName)enumClip, clip);
        }
    }

    /// <summary>
    /// Plays the audio clip with the given name
    /// </summary>
    /// <param name="name">name of the audio clip to play</param>
    public static void Play(AudioClipName name)
    {
        audioSource.PlayOneShot(audioClips[name]);
    }
}
