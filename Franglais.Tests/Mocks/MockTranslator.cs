using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Franglais.Tests.Mocks
{
    public class MockTranslator : ITranslator
    {
        public Task<string> TranslateMessage(string message, string sourceLang, string targetLang)
        {
            return Task.FromResult<string>(targetLang + ": " + message);
        }
    }
}
