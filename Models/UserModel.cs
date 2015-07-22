using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Franglais.Models
{
    public class UserModel
    {
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
        public bool IsChatting { get; set; }
        public string Language { get; set; }
        public List<string> Invites { get; set; }
    }
}