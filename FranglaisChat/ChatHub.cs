using FranglaisChat.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FranglaisChat
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

                var ids = inviter.ConnectionIds["Lobby"].Union(current.ConnectionIds["Lobby"]).ToList();

                Clients.Clients(ids).SendAsync("joinRoom", room);
            }
        }

        public List<UserModel> GetUsers()
        {
            return ConnectedUsers.ToList();
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

                Clients.Clients(invited.ConnectionIds["Lobby"].ToList()).SendAsync("inviteReceived", currentUser);
            }
        }

        public Guid JoinLobby(string name, string language, Guid? id)
        {
            if (!id.HasValue)
            {
                id = Guid.NewGuid();
            }

            var currentUser = ConnectedUsers.FirstOrDefault(cu => cu.Id == id);

            if (currentUser != null)
            {
                currentUser.Language = language;
                currentUser.UserName = name;

                if (!currentUser.ConnectionIds.Any(cids => cids.Value.Contains(Context.ConnectionId)))
                {
                    if (currentUser.ConnectionIds.Keys.Contains("Lobby"))
                    {
                        currentUser.ConnectionIds["Lobby"].Add(Context.ConnectionId);
                    }
                    else
                    {
                        currentUser.ConnectionIds.Add("Lobby", new List<string>() { Context.ConnectionId });
                    }
                }
            }
            else
            {
                var userModel = new UserModel()
                {
                    Id = id.Value,
                    ConnectionIds = new Dictionary<string, List<string>>(),
                    UserName = name,
                    Language = language,
                    IsChatting = false,
                    Invites = new List<Guid>()
                };

                userModel.ConnectionIds.Add("Lobby", new List<string>() { Context.ConnectionId });
                ConnectedUsers.Add(userModel);
                Clients.OthersInGroup("Lobby").SendAsync("userJoined", userModel);
            }

            Groups.AddToGroupAsync(Context.ConnectionId, "Lobby");

            return id.Value;
        }

        public void JoinRoom(int roomId, Guid userId)
        {
            var room = ChatRooms.FirstOrDefault(rm => rm.Id == roomId);

            if (room != null)
            {
                var current = room.Users.FirstOrDefault(cu => cu.Id == userId);

                if (current == null)
                {
                    //can a new user join the room?
                    //yes for now...need to lock down somehow
                    current = ConnectedUsers.FirstOrDefault(cu => cu.Id == userId);

                    if (current != null)
                    {
                        List<string> ids = new List<string>();
                        room.Users.ForEach(user => ids = ids.Union(user.ConnectionIds["Room" + roomId]).ToList());
                        Clients.Clients(ids).SendAsync("userJoined", current);

                        room.Users.Add(current);
                    }
                }

                if (current != null)
                {
                    if (!current.ConnectionIds.Keys.Contains("Room" + roomId))
                    {
                        current.ConnectionIds.Add("Room" + roomId, new List<string>() { Context.ConnectionId });
                    }
                    else
                    {
                        current.ConnectionIds["Room" + roomId].Add(Context.ConnectionId);
                    }
                }

                foreach (var user in room.Users.Where(usr => usr.Id != userId))
                {
                    Clients.Caller.SendAsync("userJoined", user);
                }
            }
        }

        //public override Task OnDisconnected(bool stopCalled)
        //{
        //    var currentUser = ConnectedUsers.FirstOrDefault(cu => cu.ConnectionIds.Any(cid => cid.Value.Contains(Context.ConnectionId)));

        //    if (currentUser != null)
        //    {
        //        var keyVal = currentUser.ConnectionIds.First(pair => pair.Value.Contains(Context.ConnectionId));

        //        keyVal.Value.Remove(Context.ConnectionId);
        //        if (keyVal.Value.Count == 0)
        //        {
        //            currentUser.ConnectionIds.Remove(keyVal.Key);
        //            Clients.OthersInGroup(keyVal.Key).SendAsync("userDisconnected", currentUser.Id);
        //        }

        //        if (currentUser.ConnectionIds.Count == 0)
        //        {
        //            ConnectedUsers.Remove(currentUser);
        //        }
        //    }

        //    return base.OnConnectedAsync(stopCalled);
        //}

        public async Task SendMessage(int roomId, ChatMessage mess)
        {
            var room = ChatRooms.FirstOrDefault(rm => rm.Id == roomId);

            if (room != null)
            {
                var sender = room.Users.First(cu => cu.ConnectionIds["Room" + roomId].Contains(Context.ConnectionId));

                mess.Sender = sender;
                mess.Id = messageId++;

                var sourceLang = mess.Sender.Language.Substring(0, 2);

                //should just group by language in case of >2 users
                foreach (var user in room.Users.Where(u => u != sender))
                {
                    var userLang = user.Language.Substring(0, 2);

                    if (sourceLang != userLang)
                    {
                        mess.Translation = await translator.TranslateMessage(mess.Message, sourceLang, userLang);
                    }
                    mess.ServerSent = DateTime.Now;

                    await Clients.Clients(user.ConnectionIds["Room" + roomId].ToList()).SendAsync("receiveMessage", mess);
                }
                await Clients.Clients(sender.ConnectionIds["Room" + roomId]).SendAsync("receiveMessage", mess);
            }
        }
    }
}