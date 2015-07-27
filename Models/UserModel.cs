using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Franglais.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public Dictionary<string, List<string>> ConnectionIds { get; set; }
        public string UserName { get; set; }
        public bool IsChatting { get; set; }
        public string Language { get; set; }
        public List<Guid> Invites { get; set; }
    }
}