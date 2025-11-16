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

            int totalFilesToLoad = configEntries.Count;
            int filesProcessed = 0;

            BingBongVoiceLineAPI.Log.LogInfo($"CustomSoundManager: Starting to load {totalFilesToLoad} audio clips...");

            if (totalFilesToLoad == 0)
            {
                _isLoaded = true;
                BingBongVoiceLineAPI.Log.LogInfo("CustomSoundManager: No clips to load.");
                yield break;
            }

            foreach (BingBongResponseConfigEntry entry in configEntries)
            {
                BingBongResponseConfigEntry currentEntry = entry;

                string audioPath = Path.Combine(currentEntry.configDirectory, currentEntry.file);

                StartCoroutine(AudioLoader.LoadAudioClipFromPath(audioPath, clip =>
                {
                    try
                    {
                        if (clip != null)
                        {
                            _audioClips[currentEntry.file] = clip;
                            BingBongVoiceLineAPI.Log.LogInfo($"CustomSoundManager: Loaded {currentEntry.file}");
                        }
                        else
                        {
                            BingBongVoiceLineAPI.Log.LogError($"CustomSoundManager: Failed to load {currentEntry.file}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        BingBongVoiceLineAPI.Log.LogError($"CustomSoundManager: Error in callback for {currentEntry.file}: {ex.Message}");
                    }
                    finally
                    {
                        filesProcessed++;
                    }
                }));
            }

            while (filesProcessed < totalFilesToLoad)
            {
                yield return null;
            }

            _isLoaded = true;
            BingBongVoiceLineAPI.Log.LogInfo($"CustomSoundManager: Finished loading. {_audioClips.Count} / {totalFilesToLoad} clips loaded.");
        }

        public AudioClip GetClip(string fileName)
        {
            _audioClips.TryGetValue(fileName, out AudioClip clip);
            return clip;
        }
    }
}