using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FranglaisChat.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public UserModel Sender { get; set; }
        public string Message { get; set; }
        public string Translation { get; set; }
        public DateTime ClientSent { get; set; }
        public DateTime ServerSent { get; set; }
    }
}