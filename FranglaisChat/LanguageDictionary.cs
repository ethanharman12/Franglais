using System.Collections.Generic;

namespace FranglaisChat
{
    public static class LanguageDictionary
    {
        public static Dictionary<string, string> Languages = new Dictionary<string, string>
        {
            { "en-US", "English (US)" },
            { "en-GB", "English (UK)" },
            { "fr-FR", "French (France)" },
            { "fr-CA", "French (Canada)" },
            { "es-MX", "Spanish (Mexico)" },
            { "es-ES", "Spanish (Spain)" },
            { "de-DE", "German" },
            { "it-IT", "Italian" },
            { "ru-RU", "Russian" },
            { "ja-JP", "Japanese" }
        };
    }
}
