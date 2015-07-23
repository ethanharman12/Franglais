var lobbyApp = (function ()
{
    var chatHub = $.connection.chatHub;
    var users = [];

    //private
    function AddUser(user)
    {
        users.push(user);
        DisplayUser(user);
    };
    function DisplayUser(user)
    {
        var html = '<div id="User' + user.ConnectionId + '" class="userDiv" onclick="lobbyApp.Invite(\'' + user.ConnectionId + '\')">' +
                    user.UserName +
                   '</div>';
        $("#usersDiv").append(html);

        //var userEle = document.getElementById("User" + user.ConnectionId);
        //userEle.addEventListener("click", Invite);
    };
    function DisconnectUser(name)
    {
        $("#User" + name).fadeOut(2);
    };
    function JoinRoom(room)
    {
        if (localStorage)
        {
            localStorage.connectionId = $.connection.hub.id;
        }
        window.location = "/Home/ChatRoom/" + room.Id;
    };
    function ReceiveInvite(user)
    {
        var html = '<div id="Invite' + user.ConnectionId + '" class="userDiv" onclick="lobbyApp.Accept(\'' + user.ConnectionId + '\')">' +
                    'Click to chat with ' + user.UserName + '.' +
                   '</div>';
        $("#responseDiv").append(html);

        //var userEle = document.getElementById("Invite" + user.ConnectionId);
        //userEle.addEventListener("click", Accept);
    };

    function Accept(conn)
    {
        chatHub.server.acceptChat(conn);
    };
    function Invite(conn)
    {
        chatHub.server.inviteUser(conn);
    };
    function JoinLobby()
    {
        //var lang = "English";
        //var name = "User";

        //if (localStorage)
        //{
        //    lang = localStorage.language;
        //    name = localStorage.userName;
        //}

        var lang = $("#languageDDL :selected").text();
        var name = $("#userNameTextBox").val();

        $.connection.hub.start().done(function ()
        {
            $("#joinDiv").hide();

            chatHub.server.joinLobby(name, lang);

            chatHub.server.getOtherUsers().done(function (data)
            {
                data.forEach(AddUser);
            });
        });
    };
    function SetUpHub()
    {
        chatHub.client.inviteReceived = ReceiveInvite;
        chatHub.client.joinRoom = JoinRoom;
        chatHub.client.userDisconnected = DisconnectUser;
        chatHub.client.userJoined = AddUser;
    };

    return {
        Accept: Accept,
        Invite: Invite,
        JoinLobby: JoinLobby,
        SetUpHub: SetUpHub
    };
})();

lobbyApp.SetUpHub();