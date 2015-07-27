var lobbyApp = (function ()
{
    var chatHub = $.connection.chatHub;
    var users = [];
    var userId = "";

    //private
    function AddUser(user)
    {
        users.push(user);
        DisplayUser(user);
    };
    function DisplayUser(user)
    {
        var userEle = document.getElementById("User" + user.Id);
        if (!userEle)
        {
            var html = '<div id="User' + user.Id + '" class="userDiv">' +
                        user.UserName + ' <span id="invitedCheck"></span>' +
                       '</div>';
            $("#usersDiv").append(html);
            $("#User" + user.Id).click(user.Id, Invite);
        }
    };
    function DisconnectUser(conn)
    {
        var userDiv = $("#User" + conn);
        //userDiv.fadeOut(1500);
        //interval(1500, function ()
        //{
            userDiv.remove();
        //});
    };
    function JoinRoom(room)
    {
        window.open("/Home/ChatRoom/" + room.Id, "_blank");
        //window.location = ;
    };
    function ReceiveInvite(user)
    {
        $("#User" + user.Id).off("click").click(user.Id, Accept).css("background-color", "lightgreen").addClass("preview");
        $("#User" + user.Id + " #invitedCheck").addClass("glyphicon glyphicon-ok");
    };

    function Accept(conn)
    {
        chatHub.server.acceptChat(userId, conn.data);
    };
    function Invite(conn)
    {
        $("#User" + conn.data).css("background-color", "lightgreen");
        $("#User" + conn.data + " #invitedCheck").addClass("glyphicon glyphicon-ok");

        chatHub.server.inviteUser(userId, conn.data);
    };
    function SetUpHub()
    {
        var name = localStorage.userName;
        var lang = localStorage.language;

        if (name && lang)
        {
            chatHub.client.inviteReceived = ReceiveInvite;
            chatHub.client.joinRoom = JoinRoom;
            chatHub.client.userDisconnected = DisconnectUser;
            chatHub.client.userJoined = AddUser;

            $.connection.hub.start().done(function ()
            {
                var id = localStorage.userId;
                chatHub.server.joinLobby(name, lang, id).done(function(data)
                {
                    id = data;
                    localStorage.userId = id;
                    userId = id;
                });                               

                chatHub.server.getOtherUsers().done(function (data)
                {
                    data.forEach(AddUser);
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