using System.Threading.Tasks;

namespace FranglaisChat
{
    public interface IChatBot
    {
        string SendMessage(string message);
        void SetMode(BotModeEnum botMode);
    }
}
