using System.Threading.Tasks;

namespace FranglaisChat
{
    public interface ITranslator
    {
        Task<string> TranslateMessage(string message, string sourceLang, string targetLang);
    }
}
