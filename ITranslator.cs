using System;
namespace Franglais
{
    public interface ITranslator
    {
        string TranslateMessage(string message, string language);
    }
}
