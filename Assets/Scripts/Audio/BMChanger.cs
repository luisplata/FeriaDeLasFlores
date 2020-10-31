using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BMChanger : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip1;
    [SerializeField] private AudioClip audioClip2;

    private AudioSource audioSource;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayBM());
    }

    private IEnumerator PlayBM()
    {
        while (true)
        {
            audioSource.clip = audioClip1;
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
            int random = Random.Range(1, 4);
            if (random == 2)
            {
                audioSource.clip = audioClip2;
                audioSource.Play();
                yield return new WaitForSeconds(audioSource.clip.length);
            }
        }
    }
}
