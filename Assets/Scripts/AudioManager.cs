using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource MusicAudioSource;
    public AudioSource SFXAudioSource;

    public AudioClip MainMenuMusic;
    public AudioClip GameMusic;

    public AudioClip Capture;
    public AudioClip Error;
    public AudioClip Level;

    private void Awake()
    {
        Instance = this;
    }

    internal void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        MusicAudioSource.clip = musicClip;
        MusicAudioSource.loop = loop;
        MusicAudioSource.Play();
    }

    internal void PlaySFX(AudioClip clip)
    {
        SFXAudioSource.clip = clip;
        SFXAudioSource.loop = false;
        SFXAudioSource.Play();
    }
}
