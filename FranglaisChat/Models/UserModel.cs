using System;
using System.Collections.Generic;

namespace FranglaisChat.Models
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