using System.Threading.Tasks;

namespace FranglaisChat
{
    public interface IChatBot
    {
        string SendMessage(string message);
    }
}
