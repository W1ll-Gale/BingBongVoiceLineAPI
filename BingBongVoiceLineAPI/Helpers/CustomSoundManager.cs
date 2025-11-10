using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BingBongVoiceLineAPI.Helpers
{
    public class CustomSoundManager : MonoBehaviour
    {
        public static CustomSoundManager Instance { get; private set; }

        private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
        private bool _isLoaded = false;

        public IReadOnlyDictionary<string, AudioClip> AudioClips => _audioClips;
        public bool IsLoaded => _isLoaded;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(LoadAllClips());
        }

        private IEnumerator LoadAllClips()
        {
            List<BingBongResponseConfigEntry> configEntries = ConfigLoader.LoadConfig("response_sound_pack.json");
            if (configEntries == null || configEntries.Count == 0)
            {
                BingBongVoiceLineAPI.Log.LogWarning("CustomSoundManager: No config entries found.");
                _isLoaded = true;
                yield break;
            }

            int loadedCount = 0;
            foreach (BingBongResponseConfigEntry entry in configEntries)
            {
                string audioPath = Path.Combine(entry.configDirectory, entry.file);
                yield return AudioLoader.LoadAudioClipFromPath(audioPath, clip =>
                {
                    if (clip != null)
                    {
                        _audioClips[entry.file] = clip;
                        BingBongVoiceLineAPI.Log.LogInfo($"CustomSoundManager: Loaded {entry.file}");
                    }
                    else
                    {
                        BingBongVoiceLineAPI.Log.LogError($"CustomSoundManager: Failed to load {entry.file}");
                    }
                    loadedCount++;
                });
            }

            _isLoaded = true;
            BingBongVoiceLineAPI.Log.LogInfo($"CustomSoundManager: Loaded {loadedCount} clips.");
        }

        public AudioClip GetClip(string fileName)
        {
            _audioClips.TryGetValue(fileName, out AudioClip clip);
            return clip;
        }
    }
}
