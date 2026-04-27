using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] backgroundMusics;
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float maxVolume = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        PlayRandomMusic();
    }

    private void PlayRandomMusic()
    {
        if (backgroundMusics.Length == 0) return;

        int randomIndex = Random.Range(0, backgroundMusics.Length);
        audioSource.clip = backgroundMusics[randomIndex];
        StartCoroutine(FadeIn(fadeInDuration));
    }

    private IEnumerator FadeIn(float duration)
    {
        audioSource.volume = 0f;
        audioSource.Play();

        while (audioSource.volume < maxVolume)
        {
            audioSource.volume += (maxVolume / duration) * Time.deltaTime;
            yield return null;
        }

        audioSource.volume = maxVolume;
    }

    private IEnumerator FadeOut(float duration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0f)
        {
            audioSource.volume -= (startVolume / duration) * Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    public void StopWithFadeOut()
    {
        StartCoroutine(FadeOut(fadeOutDuration));
    }
}