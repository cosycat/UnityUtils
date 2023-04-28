using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtils.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Serializable]
        public struct Sound
        {
            [SerializeField] private AudioClip audioClip;
            [SerializeField] private string name;
            // TODO add volume, pitch, etc.
            [SerializeField] private bool loop;
            [SerializeField] private float volume;
            [SerializeField] private float pitch;

            public AudioClip AudioClip => audioClip;
            public string Name => name;
            public bool Loop => loop;
            public float Volume => volume;
            public float Pitch => pitch;
            
        }

        public static AudioManager Instance { get; private set; }
        
        [SerializeField] private List<Sound> predefinedSounds = new();
        [Tooltip("The maximum number of audio sources that can be playing at once. Changes at runtime will have no effect."),
         SerializeField] private int maxAudioSources = 10;
        private readonly List<AudioSource> _audioSources = new(); // pool of audio sources
        private int _nextAudioSourceIndex = 0;
        private AudioSource GetNextAudioSource()
        {
            var audioSource = _audioSources[_nextAudioSourceIndex];
            _nextAudioSourceIndex = (_nextAudioSourceIndex + 1) % _audioSources.Count;
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
                _audioSources.Add(audioSource);
            }
        }
        
        
        public bool GetSoundByName(string name, out Sound clip)
        {
            var sound = predefinedSounds.Find(s => s.Name == name);
            if (sound.AudioClip == null)
            {
                Debug.LogWarning($"No sound with name '{name}' found");
                clip = default;
                return false;
            }
            clip = sound;
            return true;
        }
        
        public void PlaySoundByName(string name)
        {
            if (!GetSoundByName(name, out var sound)) return;
            PlaySound(sound.AudioClip, sound.Volume, sound.Pitch, sound.Loop);
        }
        
        public void PlaySoundByNameAtPosition(string name, Vector3 location)
        {
            if (!GetSoundByName(name, out var sound)) return;
            PlaySoundAtPosition(sound.AudioClip, location, sound.Volume, sound.Pitch, sound.Loop);
        }



        public void PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            var audioSource = GetNextAudioSource();
            audioSource.spatialize = false; // TODO check if this makes it stereo, without location
            // audioSource.transform.position = Vector3.zero; // not needed, but maybe cleaner
            PlaySound(audioSource, clip, volume, pitch, loop);
        }
    
        public void PlaySoundAtPosition(AudioClip clip, Vector3 location, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            // TODO locations
            var audioSource = GetNextAudioSource();
            audioSource.spatialize = true;
            audioSource.transform.position = location;
            PlaySound(audioSource, clip, volume, pitch);
        }

        private static void PlaySound(AudioSource audioSource, AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;
            audioSource.Play();
        }
    }
}
