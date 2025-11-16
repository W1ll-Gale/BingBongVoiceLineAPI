using BepInEx;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace BingBongVoiceLineAPI.Helpers
{
    public static class AudioLoader
    {
        private static string ffmpegPath = null;
        private static bool ffmpegSearched = false;
        private static string audioCachePath = Path.Combine(BepInEx.Paths.CachePath, "BingBongAudioCache");

        public static IEnumerator LoadAudioClipFromPath(string filePath, Action<AudioClip> onLoaded)
        {
            if (!File.Exists(filePath))
            {
                BingBongVoiceLineAPI.Log.LogError($"Audio file not found at path: {filePath}");
                onLoaded?.Invoke(null);
                yield break;
            }

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            AudioType? nativeType = GetNativeAudioType(extension);

            if (nativeType.HasValue)
            {
                // It's a native format, load directly
                yield return LoadNativeAudio(filePath, nativeType.Value, onLoaded);
            }
            else if (IsConversionSupported(extension))
            {
                // It's a format we can try to convert (e.g., .m4a)
                string ffmpeg = FindFFmpeg();
                if (string.IsNullOrEmpty(ffmpeg))
                {
                    BingBongVoiceLineAPI.Log.LogError($"Cannot load '{extension}' file. ffmpeg.exe was not found.");
                    onLoaded?.Invoke(null);
                    yield break;
                }

                yield return ConvertAndLoadAudio(filePath, ffmpeg, onLoaded);
            }
            else
            {
                // Unknown/unsupported format
                BingBongVoiceLineAPI.Log.LogWarning($"Unknown audio extension '{extension}'. Defaulting to WAV load attempt.");
                yield return LoadNativeAudio(filePath, AudioType.WAV, onLoaded);
            }
        }


        private static IEnumerator LoadNativeAudio(string filePath, AudioType audioType, Action<AudioClip> onLoaded)
        {
            BingBongVoiceLineAPI.Log.LogInfo($"Loading native audio from: {filePath}");
            string uri = "file://" + filePath;

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

        private static string FindFFmpeg()
        {
            if (ffmpegSearched)
            {
                return ffmpegPath;
            }

            ffmpegSearched = true;
            try
            {
                string[] foundFiles = Directory.GetFiles(BepInEx.Paths.PluginPath, "ffmpeg.exe", SearchOption.AllDirectories);

                if (foundFiles.Length == 0)
                {
                    BingBongVoiceLineAPI.Log.LogWarning("ffmpeg.exe not found anywhere in BepInEx plugin path. Non-native audio formats (e.g., .m4a) will not be loaded.");
                    return null;
                }

                ffmpegPath = foundFiles[0];
                BingBongVoiceLineAPI.Log.LogInfo($"Found ffmpeg at: {ffmpegPath}");

                if (foundFiles.Length > 1)
                {
                    BingBongVoiceLineAPI.Log.LogWarning($"Found {foundFiles.Length} instances of ffmpeg.exe. Using the first one found.");
                }

                Directory.CreateDirectory(audioCachePath);

                return ffmpegPath;
            }
            catch (Exception ex)
            {
                BingBongVoiceLineAPI.Log.LogError($"Error while searching for ffmpeg.exe: {ex.Message}");
                return null;
            }
        }

        private static IEnumerator ConvertAndLoadAudio(string sourceFilePath, string ffmpegExePath, Action<AudioClip> onLoaded)
        {
            string fileHash = GetFileHash(sourceFilePath);
            string outputWavPath = Path.Combine(audioCachePath, fileHash + ".wav");

            if (!File.Exists(outputWavPath))
            {
                BingBongVoiceLineAPI.Log.LogInfo($"Converting '{sourceFilePath}' to WAV...");

                string arguments = $"-loglevel error -i \"{sourceFilePath}\" -vn -acodec pcm_s16le -ar 44100 -ac 2 \"{outputWavPath}\"";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegExePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                StringBuilder stdOutBuilder = new StringBuilder();
                StringBuilder stdErrBuilder = new StringBuilder();

                Process process = new Process { StartInfo = startInfo };

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        stdOutBuilder.AppendLine(args.Data);
                    }
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        stdErrBuilder.AppendLine(args.Data);
                    }
                };

                bool processStarted = false;
                try
                {
                    process.Start();
                    processStarted = true;

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }
                catch (Exception ex)
                {
                    BingBongVoiceLineAPI.Log.LogError($"Exception starting FFmpeg process: {ex.Message}");
                    onLoaded?.Invoke(null);
                    yield break;
                }

                if (processStarted)
                {
                    while (!process.HasExited)
                    {
                        yield return null;
                    }

                    string stdOut = stdOutBuilder.ToString();
                    string stdErr = stdErrBuilder.ToString();

                    if (process.ExitCode != 0)
                    {
                        BingBongVoiceLineAPI.Log.LogError($"FFmpeg conversion failed with exit code {process.ExitCode}.");
                        BingBongVoiceLineAPI.Log.LogError($"FFmpeg Error: {stdErr}"); 
                        if (!string.IsNullOrWhiteSpace(stdOut)) BingBongVoiceLineAPI.Log.LogError($"FFmpeg Output: {stdOut}"); 

                        if (File.Exists(outputWavPath))
                        {
                            File.Delete(outputWavPath);
                        }

                        onLoaded?.Invoke(null);
                        yield break;
                    }
                    else
                    {
                        BingBongVoiceLineAPI.Log.LogInfo($"Successfully converted file. WAV saved at: {outputWavPath}");
                        if (!string.IsNullOrWhiteSpace(stdOut)) BingBongVoiceLineAPI.Log.LogDebug($"FFmpeg Output: {stdOut}");
                        if (!string.IsNullOrWhiteSpace(stdErr)) BingBongVoiceLineAPI.Log.LogWarning($"FFmpeg Warnings: {stdErr}");
                    }
                }
            }
            else
            {
                BingBongVoiceLineAPI.Log.LogInfo($"Found cached WAV file: {outputWavPath}");
            }

            yield return LoadNativeAudio(outputWavPath, AudioType.WAV, onLoaded);
        }

        private static AudioType? GetNativeAudioType(string extension)
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
                    return null;
            }
        }

        private static bool IsConversionSupported(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".m4a":
                    return true;
                default:
                    return false;
            }
        }

        private static string GetFileHash(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(filePath.ToLowerInvariant()));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}