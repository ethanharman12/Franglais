﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FranglaisChat.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public List<UserModel> Users { get; set; }
    }
}