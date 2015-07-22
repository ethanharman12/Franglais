using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Franglais.Models;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Franglais
{
    public class ChatHub : Hub
    {
        public static List<ChatRoom> ChatRooms = new List<ChatRoom>();
        public static List<UserModel> ConnectedUsers = new List<UserModel>();

        private static int messageId = 1;
        private static int roomId = 1;

        public ITranslator translator { get; set; }

        public ChatHub(ITranslator translator)
        {
            this.translator = translator;
        }

        public async void AcceptChat(string name)
        {
            var inviter = ConnectedUsers.FirstOrDefault(cu => cu.UserName == name);
            var current = ConnectedUsers.FirstOrDefault(cu => cu.UserName == Context.User.Identity.Name);

            if (inviter != null && current != null && inviter.Invites.Contains(current.UserName))
            {
                var room = new ChatRoom()
                    {
                        Id = roomId++,
                        Users = new List<UserModel>() { inviter, current }
                    };

                ChatRooms.Add(room);

                await Groups.Add(inviter.ConnectionId, "ChatRoom" + room.Id);
                await Groups.Add(current.ConnectionId, "ChatRoom" + room.Id);

                Clients.Group("ChatRoom" + room.Id).joinRoom(room);
            }
        }

        public List<UserModel> GetOtherUsers()
        {
            return ConnectedUsers.Where(cu => cu.UserName != Context.User.Identity.Name).ToList();
        }

        public void InviteUser(string name)
        {
            var currentUser = ConnectedUsers.First(cu => cu.UserName == Context.User.Identity.Name);

            if (!currentUser.Invites.Contains(name))
            {
                currentUser.Invites.Add(name);
            }

            Clients.User(name).inviteReceived(currentUser.UserName);
        }

        public void JoinLobby(string language)
        {
            var userModel = new UserModel()
            {
                ConnectionId = Context.ConnectionId,
                UserName = Context.User.Identity.Name,
                Language = language,
                IsChatting = false,
                Invites = new List<string>()
            };

            ConnectedUsers.Add(userModel);

            Clients.OthersInGroup("Lobby").userJoined(userModel);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var currentUser = ConnectedUsers.First(cu => cu.UserName == Context.User.Identity.Name);

            ConnectedUsers.Remove(currentUser);
            Clients.OthersInGroup("Lobby").userDisconnected(currentUser.UserName);

            return base.OnDisconnected(stopCalled);
        }

        public void SendMessage(int roomId, ChatMessage mess)
        {
            var room = ChatRooms.FirstOrDefault(rm => rm.Id == roomId);

            if (room != null)
            {
                mess.UserName = Context.User.Identity.Name;
                mess.Id = messageId++;

                foreach(var user in room.Users.Where(u => u.ConnectionId != Context.ConnectionId))
                {
                    mess.Translation = translator.TranslateMessage(mess.Message, user.Language);
                    mess.ServerSent = DateTime.Now;

                    Clients.User(user.UserName).receiveMessage(mess);
                }
                //Clients.OthersInGroup("ChatRoom" + roomId).receiveMessage(mess);
            }
        }
    }
}