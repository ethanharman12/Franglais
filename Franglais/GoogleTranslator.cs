using Franglais.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Franglais
{
    public class GoogleTranslator : ITranslator
    {
        private string GoogleAPIKey { get; set; }
        
        public GoogleTranslator()
        {
            //var keyVaultEndpoint = GetKeyVaultEndpoint();
            //if (!string.IsNullOrEmpty(keyVaultEndpoint))
            //{
            //    var azureServiceTokenProvider = new AzureServiceTokenProvider();
            //    var credentials = new ManagedIdentityCredential();
            //    var keyVaultClient = new KeyVaultClient(
            //        new KeyVaultCredential(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback))
            //        );
            //    config.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
            //}
        }

        private static string GetKeyVaultEndpoint() => "https://productionappsettings.vault.azure.net";

        public async Task<string> TranslateMessage(string message, string sourceLang, string targetLang)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var query = string.Format("language/translate/v2?key={0}&q={1}&source={2}&target={3}",
                    GoogleAPIKey,//ConfigurationManager.AppSettings["GoogleApiKey"],
                    HttpUtility.UrlEncode(message),
                    sourceLang,//.Substring(0,2),
                    targetLang);//.Substring(0,2));

                HttpResponseMessage response = await client.GetAsync(query);
                if (response.IsSuccessStatusCode)
                {
                    var translation = await response.Content.ReadAsAsync<GoogleTranslateJson>();

                    if(translation.Data != null && translation.Data.Translations != null)
                    {
                        var retString = new StringBuilder();

                        if (translation.Data.Translations.Count > 1)
                        {
                            foreach (var text in translation.Data.Translations)
                            {
                                retString.AppendLine(text.TranslatedText);
                            }
                        }
                        else
                        {
                            foreach (var text in translation.Data.Translations)
                            {
                                retString.Append(text.TranslatedText);
                            }
                        }

                        return retString.ToString();
                    }
                }
            }

            return targetLang + ": " + message;
        }
    }
}