using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Dynamic;
using Microsoft.AspNet.SignalR.Hubs;
using FranglaisChat;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNet.SignalR;
using Franglais.Tests.Mocks;
using System.Threading.Tasks;

namespace Franglais.Tests
{
    [TestClass]
    public class HubTests
    {
        private void AttachIdentity(ChatHub hub, string name, string connectionId = "1")
        {
            var mockIdentity =
                Mock.Of<ClaimsIdentity>(ci => ci.Name == name);
            var mockContext = Mock.Of<HubCallerContext>(cc => cc.User.Identity == mockIdentity
                                                           && cc.ConnectionId == connectionId);
            hub.Context = mockContext;
        }

        [TestCleanup]
        public void CleanUp()
        {
            ChatHub.ConnectedUsers = new List<UserModel>();
            ChatHub.ChatRooms = new List<ChatRoom>();
        }

        [TestMethod]
        public void GetUsers_NoUsers()
        {
            var hub = new ChatHub(new MockTranslator());

            AttachIdentity(hub, "Tester1");

            var users = hub.GetUsers();

            Assert.AreEqual(0, users.Count);
        }

        [TestMethod]
        public void GetUsers_OneLobbyOneChatting()
        {
            var guid1 = Guid.NewGuid();

            ChatHub.ConnectedUsers.Add(new UserModel()
            {
                Id = guid1,
                IsChatting = false,
                Language = "French",
                UserName = "Tester1",
                ConnectionIds = new Dictionary<string,List<string>>() { {"Lobby", new List<string>() {"C1"}}}
            });
            ChatHub.ConnectedUsers.Add(new UserModel()
            {
                IsChatting = true,
                Language = "English",
                UserName = "Tester2",
                ConnectionIds = new Dictionary<string, List<string>>() { { "Lobby", new List<string>() { "C2" } } }
            });

            var hub = new ChatHub(new MockTranslator());

            AttachIdentity(hub, "Tester1", "C1");

            var users = hub.GetUsers();

            Assert.AreEqual(2, users.Count);
            Assert.AreEqual("Tester1", users.FirstOrDefault(u => !u.IsChatting).UserName);
            Assert.AreEqual("Tester2", users.FirstOrDefault(u => u.IsChatting).UserName);
        }

        [TestMethod]
        public void JoinLobby()
        {
            UserModel userModel = new UserModel();

            var hub = new ChatHub(new MockTranslator());
            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            hub.Clients = mockClients.Object;

            var mockGroupManager = new Mock<IGroupManager>();
            hub.Groups = mockGroupManager.Object;

            dynamic all = new ExpandoObject();
            all.userJoined = new Action<UserModel>((model) =>
            {
                userModel = model;
            });

            AttachIdentity(hub, "Tester3", "C3");

            mockClients.Setup(m => m.All).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Caller).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Others).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.OthersInGroup(It.IsAny<string>())).Returns((ExpandoObject)all);

            hub.JoinLobby("Tester3", "French", null);

            Assert.AreEqual(1, ChatHub.ConnectedUsers.Count);
            Assert.AreEqual(false, userModel.IsChatting);
            Assert.AreEqual("French", userModel.Language);
            Assert.AreEqual("Tester3", userModel.UserName);
        }

        [TestMethod]
        public void Disconnect_Remove()
        {
            var guid1 = Guid.NewGuid();

            ChatHub.ConnectedUsers.Add(new UserModel()
            {
                Id = guid1,
                Language = "English",
                UserName = "Tester4",
                IsChatting = true,
                ConnectionIds = new Dictionary<string, List<string>>() { { "Lobby", new List<string>() { "C4" } } }
            });

            Guid? disUser = null;

            var hub = new ChatHub(new MockTranslator());
            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            hub.Clients = mockClients.Object;
            dynamic all = new ExpandoObject();
            all.userDisconnected = new Action<Guid>((name) =>
            {
                disUser = name;
            });

            AttachIdentity(hub, "Tester4", "C4");

            mockClients.Setup(m => m.All).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Caller).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Others).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.OthersInGroup(It.IsAny<string>())).Returns((ExpandoObject)all);

            hub.OnDisconnected(true);

            Assert.AreEqual(0, ChatHub.ConnectedUsers.Count);
            Assert.AreEqual(guid1, disUser);
        }

        [TestMethod]
        public void InviteToChat()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            ChatHub.ConnectedUsers.Add(new UserModel()
            {
                Id = guid1,
                Language = "English",
                UserName = "Tester5",
                IsChatting = false,
                Invites = new List<Guid>(),
                ConnectionIds = new Dictionary<string, List<string>>() { { "Lobby", new List<string>() { "C5" } } }
            });
            ChatHub.ConnectedUsers.Add(new UserModel()
            {
                Id = guid2,
                Language = "French",
                UserName = "Tester6",
                IsChatting = false,
                Invites = new List<Guid>(),
                ConnectionIds = new Dictionary<string, List<string>>() { { "Lobby", new List<string>() { "C6" } } }
            });

            UserModel inviter = null;

            var hub = new ChatHub(new MockTranslator());
            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            hub.Clients = mockClients.Object;
            dynamic all = new ExpandoObject();
            all.inviteReceived = new Action<UserModel>((name) =>
            {
                inviter = name;
            });

            AttachIdentity(hub, "Tester5", "C5");

            mockClients.Setup(m => m.All).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Caller).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Others).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.OthersInGroup(It.IsAny<string>())).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.User(It.IsAny<string>())).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Client(It.IsAny<string>())).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Clients(It.IsAny<List<string>>())).Returns((ExpandoObject)all);

            hub.InviteUser(guid1, guid2);

            Assert.AreEqual(guid2, ChatHub.ConnectedUsers.First(cu => cu.UserName == "Tester5").Invites[0]);
            Assert.IsNotNull(inviter);
            Assert.AreEqual(guid1, inviter.Id);
        }

        [TestMethod]
        public void AcceptChat_Initiated()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            
            ChatHub.ConnectedUsers.Add(new UserModel()
            {
                Id = guid1,
                Language = "English",
                UserName = "Tester7",
                IsChatting = false,
                Invites = new List<Guid>() { guid2 },
                ConnectionIds = new Dictionary<string, List<string>>() { { "Lobby", new List<string>() { "C7" } } }
            });
            ChatHub.ConnectedUsers.Add(new UserModel()
            {
                Id = guid2,
                Language = "French",
                UserName = "Tester8",
                IsChatting = false,
                Invites = new List<Guid>(),
                ConnectionIds = new Dictionary<string, List<string>>() { { "Lobby", new List<string>() { "C8" } } }
            });

            ChatRoom chatRoom = new ChatRoom();

            var hub = new ChatHub(new MockTranslator());
            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            hub.Clients = mockClients.Object;

            var mockGroupManager = new Mock<IGroupManager>();
            hub.Groups = mockGroupManager.Object;

            dynamic all = new ExpandoObject();
            all.joinRoom = new Action<ChatRoom>((room) =>
            {
                chatRoom = room;
            });

            AttachIdentity(hub, "Tester8", "C8");

            mockClients.Setup(m => m.All).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Caller).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Others).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.OthersInGroup(It.IsAny<string>())).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.User(It.IsAny<string>())).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Group(It.IsAny<string>())).Returns((ExpandoObject)all); 
            mockClients.Setup(m => m.Clients(It.IsAny<List<string>>())).Returns((ExpandoObject)all);

            hub.AcceptChat(guid2, guid1);

            Assert.AreEqual(1, chatRoom.Id);
            Assert.AreEqual(2, chatRoom.Users.Count);
        }

        [TestMethod]
        public void AcceptChat_Uninitiated()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            ChatHub.ConnectedUsers.Add(new UserModel()
            {
                Id = guid1,
                Language = "English",
                UserName = "Tester7",
                IsChatting = false,
                Invites = new List<Guid>(),
                ConnectionIds = new Dictionary<string, List<string>>() { { "Lobby", new List<string>() { "C7" } } }
            });
            ChatHub.ConnectedUsers.Add(new UserModel()
            {
                Id = guid2,
                Language = "French",
                UserName = "Tester8",
                IsChatting = false,
                Invites = new List<Guid>(),
                ConnectionIds = new Dictionary<string, List<string>>() { { "Lobby", new List<string>() { "C8" } } }
            });

            ChatRoom chatRoom = new ChatRoom() { Id = 0 };

            var hub = new ChatHub(new MockTranslator());
            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            hub.Clients = mockClients.Object;
            dynamic all = new ExpandoObject();
            all.startChat = new Action<ChatRoom>((room) =>
            {
                chatRoom = room;
            });

            AttachIdentity(hub, "Tester8", "C8");

            mockClients.Setup(m => m.All).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Caller).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Others).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.OthersInGroup(It.IsAny<string>())).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.User(It.IsAny<string>())).Returns((ExpandoObject)all);

            hub.AcceptChat(guid1, guid2);

            Assert.AreEqual(0, chatRoom.Id);
        }

        [TestMethod]
        public void SendMessage()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            ChatHub.ConnectedUsers.Add(new UserModel()
            {
                Id = guid1,
                Language = "English",
                UserName = "Tester9",
                IsChatting = false,
                Invites = new List<Guid>(),
                ConnectionIds = new Dictionary<string, List<string>>() { { "Room2", new List<string>() { "C9" } } }
            });
            ChatHub.ConnectedUsers.Add(new UserModel()
            {
                Id = guid2,
                Language = "French",
                UserName = "Tester10",
                IsChatting = false,
                Invites = new List<Guid>(),
                ConnectionIds = new Dictionary<string, List<string>>() { { "Room2", new List<string>() { "C10" } } }
            });

            ChatRoom chatRoom = new ChatRoom() { Id = 2, Users = ChatHub.ConnectedUsers.ToList() };
            ChatHub.ChatRooms.Add(chatRoom);

            ChatMessage mess = new ChatMessage();

            var hub = new ChatHub(new MockTranslator());
            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            hub.Clients = mockClients.Object;

            var mockGroupManager = new Mock<IGroupManager>();
            hub.Groups = mockGroupManager.Object;

            dynamic all = new ExpandoObject();
            all.receiveMessage = new Action<ChatMessage>((message) =>
            {
                mess = message;
            });

            AttachIdentity(hub, "Tester9", "C9");

            mockClients.Setup(m => m.All).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Caller).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Others).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.OthersInGroup(It.IsAny<string>())).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.User(It.IsAny<string>())).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Client(It.IsAny<string>())).Returns((ExpandoObject)all);
            mockClients.Setup(m => m.Clients(It.IsAny<List<string>>())).Returns((ExpandoObject)all);

            hub.SendMessage(2, new ChatMessage()
            {
                ClientSent = DateTime.Now,
                Message = "Hello World",
                Sender = chatRoom.Users[0]
            }).Wait();

            Assert.AreEqual(1, mess.Id);
            Assert.AreEqual("Tester9", mess.Sender.UserName);

            Assert.IsNotNull(mess.ServerSent);
            Assert.AreNotEqual(DateTime.MinValue, mess.ServerSent);
            Assert.IsTrue(mess.ServerSent > mess.ClientSent);

            Assert.AreEqual("English", mess.Sender.Language);
            Assert.AreEqual("Hello World", mess.Message);
            Assert.AreEqual("French: Hello World", mess.Translation);
        }
    }
}
