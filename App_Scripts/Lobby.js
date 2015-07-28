var lobbyApp = (function ()
{
    var chatHub = $.connection.chatHub;
    var userId = localStorage.userId;
    var name = localStorage.userName;
    var lang = localStorage.language;

    //private
    function AddUser(user)
    {
        DisplayUser(user);
    };
    function DisplayUser(user)
    {
        var userEle = document.getElementById("User" + user.Id);
        if (!userEle)
        {
            var html = '<div id="User' + user.Id + '" class="userDiv">' +
                        user.UserName + ' - ' + langDictionary.Languages[user.Language] + 
                        ' <div class="invited"><span class="invitedCheck"></span> <span class="invitedText"></span></div>' +
                       '</div>';
            $("#usersDiv").append(html);
            $("#User" + user.Id).click(user.Id, Invite);
        }
    };
    function DisconnectUser(conn)
    {
        $("#User" + conn).remove();
    };
    function JoinRoom(room)
    {
        window.open("/Home/ChatRoom/" + room.Id, "_blank");
        //window.location = ;
    };
    function ReceiveInvite(user)
    {
        $("#User" + user.Id).off("click").click(user.Id, Accept).css("background-color", "lightgreen").addClass("preview");
        $("#User" + user.Id + " .invitedCheck").addClass("glyphicon glyphicon-ok");
        $("#User" + user.Id + " .invitedText").text("Invite Received!");
    };

    function Accept(conn)
    {
        chatHub.server.acceptChat(userId, conn.data);
    };
    function Invite(conn)
    {
        $("#User" + conn.data).css("background-color", "lightcyan");
        $("#User" + conn.data + " .invitedCheck").addClass("glyphicon glyphicon-ok");
        $("#User" + conn.data + " .invitedText").text("Invite Sent!");

        chatHub.server.inviteUser(userId, conn.data);
    };
    function SetUpHub()
    {
        if (name && lang)
        {
            chatHub.client.inviteReceived = ReceiveInvite;
            chatHub.client.joinRoom = JoinRoom;
            chatHub.client.userDisconnected = DisconnectUser;
            chatHub.client.userJoined = AddUser;

            $.connection.hub.start().done(function ()
            {
                chatHub.server.joinLobby(name, lang, userId).done(function (sentId)
                {
                    localStorage.userId = sentId;
                    userId = sentId;

                    chatHub.server.getUsers().done(function (users)
                    {
                        users.forEach(function (user)
                        {
                            if (user.Id != userId)
                            {
                                AddUser(user);
                            }
                        });
                    });
                });
            });
        }
        else
        {
            window.location = "/Home/UserProfile";
        }
    };

    return {
        Accept: Accept,
        Invite: Invite,
        SetUpHub: SetUpHub
    };
})();

lobbyApp.SetUpHub();