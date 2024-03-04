using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Franglais.Models
{
    public class GoogleTranslateJson
    {
        public TranslateData Data { get; set; }
    }

    public class TranslateData
    {
        public List<TranslateTranslations> Translations { get; set; }
    }

    public class TranslateTranslations
    {
        public string TranslatedText { get; set; }
    }
}
