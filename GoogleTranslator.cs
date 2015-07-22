using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Franglais
{
    public class GoogleTranslator : ITranslator
    {
        public string TranslateMessage(string message, string language)
        {
            return language + ": " + message;
        }
    }
}