using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace FranglaisChat
{
    public class GoogleTranslator : ITranslator
    {
        private string GoogleAPIKey { get; set; }

        public GoogleTranslator(IConfiguration config)
        {
            GoogleAPIKey = config["Franglais:GoogleAPIKey"];
        }

        public async Task<string> TranslateMessage(string message, string sourceLang, string targetLang)
        {
            var targetCode = targetLang.Substring(0, 2);
            var sourceCode = sourceLang.Substring(0, 2);

            TranslationClient translationClient = new TranslationClientBuilder() { ApiKey = GoogleAPIKey }.Build();
            var response = await translationClient.TranslateTextAsync(message, targetCode, sourceCode);

            if (response.TranslatedText != null)
            {
                return response.TranslatedText;
            }

            return targetLang + ": " + message;
        }
    }
}