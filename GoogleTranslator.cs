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
        public async Task<string> TranslateMessage(string message, string sourceLang, string targetLang)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var query = string.Format("language/translate/v2?key={0}&q={1}&source={2}&target={3}",
                    "AIzaSyCq0XUawoWHjHGExmtlgkx3fmTW8AiJBa8",//ConfigurationManager.AppSettings["GoogleApiKey"],
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