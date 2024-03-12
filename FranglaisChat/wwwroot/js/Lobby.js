const { createApp, ref } = Vue

createApp({
    setup() {
        const users = ref([]);

        var chatHub = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
        var userId = localStorage.userId;
        var name = localStorage.userName;
        var lang = localStorage.language;

        function accept(inviteId) {
            chatHub.invoke("acceptChat", userId, inviteId);
        };

        function addUser(user) {
            users.value.push(user);
        };

        function disconnectUser(conn) {
            _.remove(users.value, user => user.id == conn);
        };

        function displayLanguage(language) {
            return langDictionary.Languages[language];
        };

        function invite(invitedUserId) {
            var user = _.find(users.value, user => user.id == invitedUserId);
            user.invited = true;

            chatHub.invoke("inviteUser", userId, invitedUserId);
        };
        
        function joinRoom(room) {
            room.users.forEach(function (user) {
                if (user.id != userId) {
                    var chatter = _.find(users.value, u => u.id == user.id);
                    chatter.inviteReceived = false;
                    chatter.invited = false;
                }
            });

            window.open("/Home/ChatRoom/" + room.id, "_blank");
        };

        function receiveInvite(inviter) {
            var user = _.find(users.value, u => u.id == inviter.id);
            user.inviteReceived = true;
        };
        
        function setUpHub() {
            if (name && lang) {
                chatHub.on("inviteReceived", receiveInvite);
                chatHub.on("joinRoom", joinRoom);
                chatHub.on("userDisconnected", disconnectUser);
                chatHub.on("userJoined", addUser);

                chatHub.start().then(function () {
                    chatHub.invoke("joinLobby", name, lang, userId).then(function (sentId) {
                        localStorage.userId = sentId;
                        userId = sentId;

                        chatHub.invoke("getUsers").then(function (lobbyUsers) {
                            lobbyUsers.forEach(function (user) {
                                if (user.id != userId) {
                                    addUser(user);
                                }
                            });
                        });
                    });
                });

            }
            else {
                window.location = "/Home/UserProfile";
            }
        };

        setUpHub();

        return {
            accept,
            displayLanguage,
            invite,
            users
        }
    }
}).mount('#usersDiv')