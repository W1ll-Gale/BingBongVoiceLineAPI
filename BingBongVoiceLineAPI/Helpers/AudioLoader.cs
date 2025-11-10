using BepInEx;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace BingBongVoiceLineAPI.Helpers
{
    public static class AudioLoader
    {
        public static IEnumerator LoadAudioClipFromPath(string filePath, Action<AudioClip> onLoaded)
        {
            BingBongVoiceLineAPI.Log.LogInfo($"Loading audio from: {filePath}");

            string uri = "file://" + filePath;
            AudioType audioType = GetAudioTypeFromExtension(Path.GetExtension(filePath));

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, audioType))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    onLoaded?.Invoke(clip);
                }
                else
                {
                    BingBongVoiceLineAPI.Log.LogError($"Failed to load audio: {www.error}");
                    onLoaded?.Invoke(null);
                }
            }
        }

        private static AudioType GetAudioTypeFromExtension(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".wav":
                    return AudioType.WAV;
                case ".mp3":
                    return AudioType.MPEG;
                case ".ogg":
                    return AudioType.OGGVORBIS;
                case ".aiff":
                case ".aif":
                    return AudioType.AIFF;
                case ".xm":
                    return AudioType.XM;
                case ".mod":
                    return AudioType.MOD;
                case ".it":
                    return AudioType.IT;
                case ".s3m":
                    return AudioType.S3M;
                default:
                    BingBongVoiceLineAPI.Log.LogWarning($"Unknown audio extension '{extension}', defaulting to WAV.");
                    return AudioType.WAV;
            }
        }
    }
}
