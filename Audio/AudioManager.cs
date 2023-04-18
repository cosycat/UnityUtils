using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [SerializeField] private int maxAudioSources = 10;
    private List<AudioSource> audioSources = new List<AudioSource>();
    private int nextAudioSourceIndex = 0;
    private AudioSource GetNextAudioSource()
    {
        var audioSource = audioSources[nextAudioSourceIndex];
        nextAudioSourceIndex = (nextAudioSourceIndex + 1) % audioSources.Count;
        return audioSource;
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        for (int i = 0; i < maxAudioSources; i++)
        {
            var audioSource = new GameObject($"AudioSource{i}").AddComponent<AudioSource>();
            audioSources.Add(audioSource);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        // TODO locations
        var audioSource = GetNextAudioSource();
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}
