using BepInEx.Configuration;

namespace BingBongVoiceLineAPI.Config
{
    internal class Configuration
    {
        public static ConfigEntry<bool> EnableMod;
        public static ConfigEntry<bool> ReplaceBingBongResponses;

        public static void Init(ConfigFile config)
        {
            EnableMod = config.Bind<bool>(
                "General",
                "Bing Bong Voice Line API",
                true,
                "If enabled, the Bing Bong Voice Line API mod will be active."
            );
            ReplaceBingBongResponses = config.Bind<bool>(
                "General",
                "Replace Bing Bong Responses",
                true,
                "If enabled, custom responses will replace the default Bing Bong responses instead of being added to them."
            );
        }
    }
}
