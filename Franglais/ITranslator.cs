using System;
using System.Threading.Tasks;
namespace Franglais
{
    public interface ITranslator
    {
        Task<string> TranslateMessage(string message, string sourceLang, string targetLang);
    }
}
