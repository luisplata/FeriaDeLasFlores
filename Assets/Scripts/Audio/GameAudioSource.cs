using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Audio source for the entire game
/// </summary>
public class GameAudioSource : MonoBehaviour
{
    private void Awake()
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        AudioManager.Initialize(audioSource);
    }
}
