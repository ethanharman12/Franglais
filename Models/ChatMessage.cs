using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Franglais.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public string Translation { get; set; }
        public string Language { get; set; }
        public DateTime ClientSent { get; set; }
        public DateTime ServerSent { get; set; }
    }
}