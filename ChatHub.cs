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

        public void AcceptChat(Guid acceptId, Guid inviteId)
        {
            var inviter = ConnectedUsers.FirstOrDefault(cu => cu.Id == inviteId);
            var current = ConnectedUsers.FirstOrDefault(cu => cu.Id == acceptId);

            if (inviter != null && current != null && inviter.Invites.Contains(current.Id))
            {
                var room = new ChatRoom()
                    {
                        Id = roomId++,
                        Users = new List<UserModel>() { inviter, current }
                    };

                ChatRooms.Add(room);

                //await Groups.Add(inviter.ConnectionId, "ChatRoom" + room.Id);
                //await Groups.Add(current.ConnectionId, "ChatRoom" + room.Id);

                //Clients.Group("ChatRoom" + room.Id).joinRoom(room);
                Clients.Clients(new List<string>() { inviter.ConnectionIds["Lobby"], current.ConnectionIds["Lobby"] }).joinRoom(room);
            }
        }

        public List<UserModel> GetOtherUsers()
        {
            return ConnectedUsers.Where(cu => cu.ConnectionIds["Lobby"] != Context.ConnectionId).ToList();
        }

        public void InviteUser(Guid fromId, Guid toId)
        {
            var currentUser = ConnectedUsers.First(cu => cu.Id == fromId);
            var invited = ConnectedUsers.FirstOrDefault(cu => cu.Id == toId);

            if (invited != null)
            {
                if (!currentUser.Invites.Contains(toId))
                {
                    currentUser.Invites.Add(toId);
                }

                Clients.Client(invited.ConnectionIds["Lobby"]).inviteReceived(currentUser);
            }
        }

        public Guid JoinLobby(string name, string language, Guid? id)
        {
            if (!id.HasValue)
            {
                id = Guid.NewGuid();
            }

            var userModel = new UserModel()
            {
                Id = id.Value,
                ConnectionIds = new Dictionary<string, string>(),
                UserName = name,
                Language = language,
                IsChatting = false,
                Invites = new List<Guid>()
            };

            userModel.ConnectionIds.Add("Lobby", Context.ConnectionId);

            ConnectedUsers.Add(userModel);

            Groups.Add(Context.ConnectionId, "Lobby");
            Clients.OthersInGroup("Lobby").userJoined(userModel);

            return id.Value;
        }

        public void JoinRoom(int roomId, Guid userId)
        {
            var room = ChatRooms.FirstOrDefault(rm => rm.Id == roomId);

            if (room != null)
            {
                var current = room.Users.First(cu => cu.Id == userId);

                if (current != null)
                {
                    if (!current.ConnectionIds.Keys.Contains("Room" + roomId))
                    {
                        current.ConnectionIds.Add("Room" + roomId, Context.ConnectionId);
                    }
                    else
                    {
                        current.ConnectionIds["Room" + roomId] = Context.ConnectionId;
                    }
                }
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var currentUser = ConnectedUsers.FirstOrDefault(cu => cu.ConnectionIds.Any(cid => cid.Value == Context.ConnectionId));

            if (currentUser != null)
            {
                ConnectedUsers.Remove(currentUser);
                Clients.OthersInGroup("Lobby").userDisconnected(currentUser.Id);
            }

            return base.OnDisconnected(stopCalled);
        }

        public async Task SendMessage(int roomId, ChatMessage mess)
        {
            var room = ChatRooms.FirstOrDefault(rm => rm.Id == roomId);

            if (room != null)
            {
                var sender = room.Users.First(cu => cu.ConnectionIds["Room" + roomId] == Context.ConnectionId);

                mess.Sender = sender;
                mess.Id = messageId++;

                //should just group by language in case of >2 users
                foreach (var user in room.Users.Where(u => u != sender))
                {
                    if (mess.Sender.Language != user.Language)
                    {
                        mess.Translation = await translator.TranslateMessage(mess.Message, mess.Sender.Language, user.Language);
                    }
                    mess.ServerSent = DateTime.Now;

                    Clients.Client(user.ConnectionIds["Room" + roomId]).receiveMessage(mess);
                }
                Clients.Caller.receiveMessage(mess);
            }
        }
    }
}