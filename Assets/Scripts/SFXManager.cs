using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SFXPair
{
    public SFXType type;
    public AudioClip clip;
}

public enum SFXType
{
    Jump,
    Hit,
    Explosion,
    Buy,
    // Add more SFX types as needed
}

public class SFXManager : MonoBehaviour
{
    [SerializeField]
    private List<SFXPair> sfxPairs = new List<SFXPair>();
    private Dictionary<SFXType, AudioClip> sfxLibrary;
    private List<AudioSource> audioSourcePool;
    private int poolSize = 10; // Adjust pool size based on expected needs
    static public SFXManager instance;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            return;
        }
        Destroy(this.gameObject);

    }

    private void Awake()
    {
        InitializeSFXLibrary();
        InitializeAudioSourcePool();
    }

    private void InitializeSFXLibrary()
    {
        sfxLibrary = new Dictionary<SFXType, AudioClip>();
        foreach (SFXPair pair in sfxPairs)
        {
            sfxLibrary[pair.type] = pair.clip;
        }
    }

    private void InitializeAudioSourcePool()
    {
        audioSourcePool = new List<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioSourcePool.Add(source);
        }
    }

    public void PlaySFX(SFXType type)
    {
        if (sfxLibrary.ContainsKey(type))
        {
            AudioSource source = GetAvailableAudioSource();
            if (source != null)
            {
                source.clip = sfxLibrary[type];
                // Set the pitch with a random variation of 10%
                source.pitch = 1.0f + UnityEngine.Random.Range(-0.1f, 0.1f);
                // Set the volume to maximum
                source.volume = .1f;
                source.Play();
            }
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource source in audioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // Optionally expand the pool if no source is available
        return ExpandPool();
    }

    private AudioSource ExpandPool()
    {
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        audioSourcePool.Add(newSource);
        return newSource;
    }
}
