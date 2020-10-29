using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioListenerStopper : MonoBehaviour
{
    private AudioListener audioListener;

    private void Start()
    {
        audioListener = GetComponent<AudioListener>();
        EventManager.AddListener(EventName.GameOverEvent, HandleGameOver);
    }
    private void HandleGameOver(int unused)
    {
        audioListener.enabled = false;
    }
}
