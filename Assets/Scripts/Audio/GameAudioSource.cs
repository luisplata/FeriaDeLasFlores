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
        //Make sure we only have one of this game object in the game
        if (!AudioManager.Initialized)
        {
            //Initialize audio manager and persist audio source across the game
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            AudioManager.Initialize(audioSource);
            DontDestroyOnLoad(gameObject);
        }
        else{
            //destriy if this game object is duplicated
            Destroy(gameObject);
        }
    }
}
