using UnityEngine;
using System.Collections.Generic;

namespace PixelGame
{
    /// <summary>
    /// Manages all audio (music and sound effects) in the game
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private int maxSFXSources = 10;

        [Header("Audio Clips")]
        [SerializeField] private List<AudioClipEntry> musicClips = new List<AudioClipEntry>();
        [SerializeField] private List<AudioClipEntry> sfxClips = new List<AudioClipEntry>();

        [Header("Settings")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

        [System.Serializable]
        public class AudioClipEntry
        {
            public string name;
            public AudioClip clip;
            [Range(0f, 1f)]
            public float volume = 1f;
        }

        // SFX pooling
        private List<AudioSource> sfxSourcePool = new List<AudioSource>();
        private Dictionary<string, AudioClip> musicDictionary = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioClipEntry> sfxDictionary = new Dictionary<string, AudioClipEntry>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Subscribe to events
            GameEvents.OnPlaySoundEffect += PlaySoundEffect;
            GameEvents.OnMusicChanged += PlayMusic;
        }

        private void OnDestroy()
        {
            GameEvents.OnPlaySoundEffect -= PlaySoundEffect;
            GameEvents.OnMusicChanged -= PlayMusic;
        }

        private void InitializeAudio()
        {
            // Create audio sources if not assigned
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            // Create SFX pool
            for (int i = 0; i < maxSFXSources; i++)
            {
                GameObject sourceObj = new GameObject($"SFXSource_{i}");
                sourceObj.transform.SetParent(transform);
                AudioSource source = sourceObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxSourcePool.Add(source);
            }

            // Build dictionaries
            foreach (var entry in musicClips)
            {
                if (!musicDictionary.ContainsKey(entry.name))
                {
                    musicDictionary.Add(entry.name, entry.clip);
                }
            }

            foreach (var entry in sfxClips)
            {
                if (!sfxDictionary.ContainsKey(entry.name))
                {
                    sfxDictionary.Add(entry.name, entry);
                }
            }
        }

        #region Music

        /// <summary>
        /// Play music track by name
        /// </summary>
        public void PlayMusic(string musicName)
        {
            if (string.IsNullOrEmpty(musicName) || !musicDictionary.ContainsKey(musicName))
            {
                Debug.LogWarning($"Music '{musicName}' not found!");
                return;
            }

            AudioClip clip = musicDictionary[musicName];

            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.volume = masterVolume * musicVolume;
            musicSource.Play();
        }

        /// <summary>
        /// Stop current music
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }

        /// <summary>
        /// Pause current music
        /// </summary>
        public void PauseMusic()
        {
            musicSource.Pause();
        }

        /// <summary>
        /// Resume paused music
        /// </summary>
        public void ResumeMusic()
        {
            musicSource.UnPause();
        }

        #endregion

        #region Sound Effects

        /// <summary>
        /// Play sound effect by name
        /// </summary>
        public void PlaySoundEffect(string sfxName, Vector3 position = default)
        {
            if (string.IsNullOrEmpty(sfxName) || !sfxDictionary.ContainsKey(sfxName))
            {
                Debug.LogWarning($"SFX '{sfxName}' not found!");
                return;
            }

            AudioClipEntry entry = sfxDictionary[sfxName];

            // Get available source from pool
            AudioSource source = GetAvailableSFXSource();

            if (source != null)
            {
                source.clip = entry.clip;
                source.volume = masterVolume * sfxVolume * entry.volume;
                source.transform.position = position;
                source.Play();
            }
        }

        /// <summary>
        /// Play sound effect at a specific location with 3D audio
        /// </summary>
        public void PlaySoundEffect3D(string sfxName, Vector3 position, float spatialBlend = 1f)
        {
            if (string.IsNullOrEmpty(sfxName) || !sfxDictionary.ContainsKey(sfxName))
            {
                return;
            }

            AudioClipEntry entry = sfxDictionary[sfxName];
            AudioSource source = GetAvailableSFXSource();

            if (source != null)
            {
                source.clip = entry.clip;
                source.volume = masterVolume * sfxVolume * entry.volume;
                source.transform.position = position;
                source.spatialBlend = spatialBlend;
                source.Play();
            }
        }

        private AudioSource GetAvailableSFXSource()
        {
            // Find inactive source
            foreach (var source in sfxSourcePool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // All busy, return first (will interrupt)
            return sfxSourcePool.Count > 0 ? sfxSourcePool[0] : sfxSource;
        }

        #endregion

        #region Volume Control

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        private void UpdateVolumes()
        {
            if (musicSource != null)
            {
                musicSource.volume = masterVolume * musicVolume;
            }
        }

        #endregion
    }
}
