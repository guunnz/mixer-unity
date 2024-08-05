using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MusicTrackPair
{
    public MusicTrack track;
    public AudioClip clip;
}

public enum MusicTrack
{
    Shldslep,
    Ridthabus,
    GO,
    PunchiEpic,
    NojyHypehu,
    Tululu,
    I2Saintz,
    Laingved,
    NeeEtheAhPehro
    // Add more tracks as needed
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField]
    private List<MusicTrackPair> musicPairs = new List<MusicTrackPair>();

    private AudioSource audioSource;
    private Dictionary<MusicTrack, AudioClip> trackLibrary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
        InitializeTrackLibrary();
    }

    private void InitializeTrackLibrary()
    {
        trackLibrary = new Dictionary<MusicTrack, AudioClip>();
        foreach (MusicTrackPair pair in musicPairs)
        {
            trackLibrary[pair.track] = pair.clip;
        }
    }

    public void PlayMusic(MusicTrack track, float fadeDuration = 1.0f)
    {
        if (trackLibrary.ContainsKey(track))
        {
            StartCoroutine(FadeMusic(track, fadeDuration));
        }
    }

    public void FadeOut(float duration)
    {
        StartCoroutine(FadeOutMusic(duration));
    }

    public void Stop()
    {
        audioSource.Stop();
        audioSource.volume = 0;
    }

    private IEnumerator FadeMusic(MusicTrack newTrack, float duration)
    {
        if (audioSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic(duration / 2));
        }

        audioSource.clip = trackLibrary[newTrack];
        audioSource.Play();
        yield return StartCoroutine(FadeInMusic(duration / 2));
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = 0;
    }

    private IEnumerator FadeInMusic(float duration)
    {
        float targetVolume = .2f;
        audioSource.volume = 0;

        while (audioSource.volume < targetVolume - 0.04)
        {
            audioSource.volume += targetVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}
