using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BingBongVoiceLineAPI.Helpers
{
    public static class ConfigLoader
    {
        public static List<BingBongResponseConfigEntry> LoadConfig(string configFileName)
        {
            List<BingBongResponseConfigEntry> allEntries = new List<BingBongResponseConfigEntry>();
            string[] foundFiles = Directory.GetFiles(BepInEx.Paths.PluginPath, configFileName, SearchOption.AllDirectories);

            if (foundFiles.Length == 0)
            {
                BingBongVoiceLineAPI.Log.LogWarning($"No config files named '{configFileName}' found in '{BepInEx.Paths.PluginPath}'.");
                return allEntries;
            }

            foreach (string filePath in foundFiles)
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    BingBongResponseConfigFile configFile = JsonConvert.DeserializeObject<BingBongResponseConfigFile>(json);
                    if (configFile?.entries != null)
                    {
                        string configDir = Path.GetDirectoryName(filePath);
                        foreach (BingBongResponseConfigEntry entry in configFile.entries)
                        {
                            entry.configDirectory = configDir;
                            entry.modName = configFile.name;
                            allEntries.Add(entry);
                        }
                        BingBongVoiceLineAPI.Log.LogInfo($"Loaded config for mod '{configFile.name}' from with {configFile.entries.Count} entries.");
                    }
                    else
                    {
                        BingBongVoiceLineAPI.Log.LogWarning($"Config file '{filePath}' is missing entries or mod name.");
                    }
                }
                catch (Exception ex)
                {
                    BingBongVoiceLineAPI.Log.LogError($"Error loading config file '{filePath}': {ex.Message}");
                }
            }

            allEntries.Sort((a, b) =>
            {
                int modCompare = string.Compare(a.modName, b.modName, StringComparison.Ordinal);
                if (modCompare != 0) return modCompare;
                int fileCompare = string.Compare(a.file, b.file, StringComparison.Ordinal);
                if (fileCompare != 0) return fileCompare;
                return string.Compare(a.subtitle, b.subtitle, StringComparison.Ordinal);
            });

            return allEntries;
        }
    }

    [Serializable]
    public class BingBongResponseConfigFile
    {
        public string name;
        public List<BingBongResponseConfigEntry> entries;
    }

    [Serializable]
    public class BingBongResponseConfigEntry
    {
        public string file;
        public string subtitle;

        [NonSerialized]
        public string configDirectory;

        [NonSerialized]
        public string modName;
    }
}
