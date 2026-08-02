using System.Collections;
using UnityEngine;

public class BMChanger : MonoBehaviour
{
    [Header("Clips de audio")]
    [SerializeField] private AudioClip audioClip1;
    [SerializeField] private AudioClip audioClip2;

    [Header("Probabilidad")]
    [Range(0f, 1f)]
    [SerializeField] private float clip1Probability = 0.5f; // 50% clip1, 50% clip2

    private AudioSource audioSource;
    private Coroutine musicCoroutine;
    private bool isPlaying = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("BMChanger: No se encontró AudioSource en el GameObject.");
            enabled = false;
            return;
        }

        // Iniciar la música
        PlayBackgroundMusic();
    }

    /// <summary>
    /// Inicia o reinicia la música de fondo.
    /// </summary>
    public void PlayBackgroundMusic()
    {
        // Detener corrutina anterior si existe
        if (musicCoroutine != null)
            StopCoroutine(musicCoroutine);

        isPlaying = true;
        musicCoroutine = StartCoroutine(PlayMusicRoutine());
    }

    /// <summary>
    /// Detiene la música de fondo.
    /// </summary>
    public void StopBackgroundMusic()
    {
        isPlaying = false;
        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
            musicCoroutine = null;
        }
        if (audioSource != null)
            audioSource.Stop();
    }

    private IEnumerator PlayMusicRoutine()
    {
        // ---- Reproducción inicial ----
        if (audioClip1 != null)
        {
            PlayClip(audioClip1);
            yield return new WaitForSeconds(audioClip1.length);
        }
        else
        {
            Debug.LogWarning("audioClip1 no asignado. Se omite la reproducción inicial.");
            // Si no hay clip1, intentar con clip2 como inicial
            if (audioClip2 != null)
            {
                PlayClip(audioClip2);
                yield return new WaitForSeconds(audioClip2.length);
            }
            else
            {
                Debug.LogError("Ambos clips son nulos. No se reproducirá música.");
                yield break;
            }
        }

        // ---- Bucle principal ----
        while (isPlaying)
        {
            // Elegir el siguiente clip según la probabilidad
            AudioClip nextClip = (Random.value < clip1Probability) ? audioClip1 : audioClip2;

            // Si el clip elegido es nulo, usar el otro como fallback
            if (nextClip == null)
            {
                nextClip = (audioClip1 != null) ? audioClip1 : audioClip2;
                if (nextClip == null)
                {
                    Debug.LogError("Ambos clips son nulos. Deteniendo música.");
                    yield break;
                }
            }

            PlayClip(nextClip);
            yield return new WaitForSeconds(nextClip.length);
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.clip = clip;
        audioSource.Play();
    }

    // Al desactivar el objeto, detener la música para evitar corrutinas huérfanas
    private void OnDisable()
    {
        StopBackgroundMusic();
    }
}