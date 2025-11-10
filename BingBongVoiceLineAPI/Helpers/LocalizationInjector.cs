using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingBongVoiceLineAPI.Helpers
{
    public static class LocalizationInjector
    {
        public static void AddCustomSubtitle(string id, string text)
        {
            LocalizedText.TryInitTables();

            List<string> translations = new List<string>();
            for (int i = 0; i < LocalizedText.LANGUAGE_COUNT; i++)
                translations.Add(text);

            LocalizedText.mainTable[id.ToUpperInvariant()] = translations;

            BingBongVoiceLineAPI.Log.LogInfo($"Added custom subtitle '{id}' to localization table.");
        }
    }
}
