using FranglaisChat.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
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
        public static Dictionary<Guid, IChatBot> ChatBots = new Dictionary<Guid, IChatBot>();
        public static bool Initialized = false;

        private static int _messageId = 1;
        private static int _roomId = 1;

        public ITranslator translator { get; set; }

        public ChatHub(ITranslator translator, IConfiguration config)
        {
            this.translator = translator;

            if (!Initialized)
            {
                AddChatBots(config);

                Initialized = true;
            }
        }

        private void AddChatBots(IConfiguration config)
        {
            foreach (var lang in LanguageDictionary.Languages)
            {
                var chatBot = new UserModel
                {
                    Id = Guid.NewGuid(),
                    IsChatBot = true,
                    IsChatting = false,
                    ChatLanguage = lang.Key,
                    NativeLanguage = lang.Key,
                    UserName = lang.Value,
                    ConnectionIds = new Dictionary<string, List<string>>(),
                    Invites = new List<Guid>()
                };
                chatBot.ConnectionIds.Add("Lobby", new List<string>());

                ConnectedUsers.Add(chatBot);
                ChatBots.Add(chatBot.Id, new ChatBot(config));
            }
        }

        private ChatRoom CreateRoom(List<UserModel> users)
        {
            var room = new ChatRoom()
            {
                Id = _roomId++,
                Users = users
            };

            ChatRooms.Add(room);

            var ids = users.SelectMany(u => u.ConnectionIds["Lobby"]).Distinct();

            Clients.Clients(ids).SendAsync("joinRoom", room);

            return room;
        }

        public void AcceptChat(Guid acceptId, Guid inviteId)
        {
            var inviter = ConnectedUsers.FirstOrDefault(cu => cu.Id == inviteId);
            var current = ConnectedUsers.FirstOrDefault(cu => cu.Id == acceptId);

            if (inviter != null && current != null && inviter.Invites.Contains(current.Id))
            {
                CreateRoom(new List<UserModel> { inviter, current });
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
                if (invited.IsChatBot)
                {
                    var room = CreateRoom(new List<UserModel> { currentUser, invited });
                    invited.ConnectionIds.Add("Room" + room.Id, new List<string>());
                }
                else
                {
                    if (!currentUser.Invites.Contains(toId))
                    {
                        currentUser.Invites.Add(toId);
                    }

                    Clients.Clients(invited.ConnectionIds["Lobby"].ToList()).SendAsync("inviteReceived", currentUser);
                }
            }
        }

        public Guid JoinLobby(string name, string chatLanguage, string nativeLanguage, Guid? id)
        {
            if (!id.HasValue)
            {
                id = Guid.NewGuid();
            }

            var currentUser = ConnectedUsers.FirstOrDefault(cu => cu.Id == id);

            if (currentUser != null)
            {
                currentUser.ChatLanguage = chatLanguage;
                currentUser.NativeLanguage = nativeLanguage;
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
                    ChatLanguage = chatLanguage,
                    NativeLanguage = nativeLanguage,
                    IsChatBot = false,
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

        private async Task SendMessageToOthers(int roomId, ChatMessage mess, UserModel sender, List<UserModel> receivers)
        {
            //should just group by language in case of >2 users
            foreach (var user in receivers)
            {
                if (mess.Sender.ChatLanguage != user.ChatLanguage)
                {
                    mess.Message = await translator.TranslateMessage(mess.Message, mess.Sender.ChatLanguage, user.ChatLanguage);
                    mess.Translation = await translator.TranslateMessage(mess.Message, mess.Sender.ChatLanguage, user.NativeLanguage);
                }
                else
                {
                    if (user.ChatLanguage != user.NativeLanguage)
                    {
                        mess.Translation = await translator.TranslateMessage(mess.Message, mess.Sender.ChatLanguage, user.NativeLanguage);
                    }
                }

                mess.ServerSent = DateTime.Now;

                if (user.IsChatBot)
                {
                    var response = ChatBots[user.Id].SendMessage(mess.Message);
                    if (response != null)
                    {
                        //only for 1-1 chats
                        var responseMess = new ChatMessage
                        {
                            Id = _messageId++,
                            Message = response,
                            Original = response,
                            ClientSent = DateTime.Now,
                            Sender = user
                        };
                        await SendMessageToOthers(roomId, responseMess, user, new List<UserModel> { sender });
                    }
                }
                else
                {
                    await Clients.Clients(user.ConnectionIds["Room" + roomId].ToList()).SendAsync("receiveMessage", mess);
                }
            }
        }

        public async Task SendMessage(int roomId, ChatMessage mess)
        {
            var room = ChatRooms.FirstOrDefault(rm => rm.Id == roomId);

            if (room != null)
            {
                var sender = room.Users.First(cu => cu.ConnectionIds["Room" + roomId].Contains(Context.ConnectionId));

                mess.Sender = sender;
                mess.Id = _messageId++;
                mess.Original = mess.Message;

                //should this just be done client side?
                if (sender.ChatLanguage != sender.NativeLanguage)
                {
                    //this gets very messy overwriting translations to send to other clients - refactor
                    mess.Translation = await translator.TranslateMessage(mess.Message, sender.ChatLanguage, sender.NativeLanguage);
                }
                await Clients.Clients(sender.ConnectionIds["Room" + roomId]).SendAsync("receiveMessage", mess);

                await SendMessageToOthers(roomId, mess, sender, room.Users.Except(new List<UserModel> { sender }).ToList());
            }
        }
    }
}