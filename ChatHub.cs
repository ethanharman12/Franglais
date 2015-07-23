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

        public async Task AcceptChat(string conn)
        {
            var inviter = ConnectedUsers.FirstOrDefault(cu => cu.ConnectionId == conn);
            var current = ConnectedUsers.FirstOrDefault(cu => cu.ConnectionId == Context.ConnectionId);

            if (inviter != null && current != null && inviter.Invites.Contains(current.ConnectionId))
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
            return ConnectedUsers.Where(cu => cu.ConnectionId != Context.ConnectionId).ToList();
        }

        public void InviteUser(string conn)
        {
            var currentUser = ConnectedUsers.First(cu => cu.ConnectionId == Context.ConnectionId);

            if (!currentUser.Invites.Contains(conn))
            {
                currentUser.Invites.Add(conn);
            }

            Clients.Client(conn).inviteReceived(currentUser);
        }

        public void JoinLobby(string name, string language)
        {
            var userModel = new UserModel()
            {
                ConnectionId = Context.ConnectionId,
                UserName = name,
                Language = language,
                IsChatting = false,
                Invites = new List<string>()
            };

            ConnectedUsers.Add(userModel);

            Groups.Add(Context.ConnectionId, "Lobby");
            Clients.OthersInGroup("Lobby").userJoined(userModel);
        }

        public void JoinRoom(int roomId, string oldConn)
        {
            var room = ChatRooms.FirstOrDefault(rm => rm.Id == roomId);

            if (room != null)
            {
                var current = room.Users.First(cu => cu.ConnectionId == oldConn);

                if(current != null)
                {
                    current.ConnectionId = Context.ConnectionId;

                    ConnectedUsers.Add(current);
                }                
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var currentUser = ConnectedUsers.FirstOrDefault(cu => cu.ConnectionId == Context.ConnectionId);

            if (currentUser != null)
            {
                ConnectedUsers.Remove(currentUser);
                Clients.OthersInGroup("Lobby").userDisconnected(currentUser.UserName);
            }

            return base.OnDisconnected(stopCalled);
        }

        public async Task SendMessage(int roomId, ChatMessage mess)
        {
            var room = ChatRooms.FirstOrDefault(rm => rm.Id == roomId);

            if (room != null)
            {
                var sender = room.Users.First(cu => cu.ConnectionId == Context.ConnectionId);

                mess.Sender = sender;
                mess.Id = messageId++;

                //should just group by language in case of >2 users
                foreach(var user in room.Users.Where(u => u.ConnectionId != Context.ConnectionId))
                {
                    mess.Translation = await translator.TranslateMessage(mess.Message, mess.Sender.Language, user.Language);
                    mess.ServerSent = DateTime.Now;

                    Clients.Client(user.ConnectionId).receiveMessage(mess);
                }
                Clients.Caller.receiveMessage(mess);
            }
        }
    }
}