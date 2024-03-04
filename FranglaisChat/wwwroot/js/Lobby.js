var lobbyApp = (function ()
{
    var chatHub = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
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
            var html = '<div id="User' + user.id + '" class="userDiv">' +
                        user.userName + ' - ' + langDictionary.Languages[user.language] + 
                        ' <div class="invited"><span class="invitedCheck"></span> <span class="invitedText"></span></div>' +
                       '</div>';
            $("#usersDiv").append(html);
            $("#User" + user.id).click(user.id, Invite);
        }
    };
    function DisconnectUser(conn)
    {
        $("#User" + conn).remove();
    };
    function JoinRoom(room)
    {
        room.users.forEach(function (user)
        {
            $("#User" + user.id).off("click").click(user.id, Invite).css("background-color", "lightblue").removeClass("preview");
            $("#User" + user.id + " .invitedCheck").removeClass("glyphicon glyphicon-ok");
            $("#User" + user.id + " .invitedText").text("");
        });

        window.open("/Home/ChatRoom/" + room.id, "_blank");
        //window.location = ;
    };
    function ReceiveInvite(user)
    {
        $("#User" + user.id).off("click").click(user.id, Accept).css("background-color", "lightgreen").addClass("preview");
        $("#User" + user.id + " .invitedCheck").addClass("glyphicon glyphicon-ok");
        $("#User" + user.id + " .invitedText").text("Invite Received!");
    };

    function Accept(clickEvent)
    {
        chatHub.invoke("acceptChat", userId, clickEvent.data);
    };
    function Invite(clickEvent)
    {
        var invitedUserId = clickEvent.data

        $("#User" + invitedUserId).css("background-color", "lightcyan");
        $("#User" + invitedUserId + " .invitedCheck").addClass("glyphicon glyphicon-ok");
        $("#User" + invitedUserId + " .invitedText").text("Invite Sent!");

        chatHub.invoke("inviteUser", userId, invitedUserId);
    };
    function SetUpHub()
    {
        if (name && lang)
        {
            chatHub.on("inviteReceived", ReceiveInvite);
            chatHub.on("joinRoom", JoinRoom);
            chatHub.on("userDisconnected", DisconnectUser);
            chatHub.on("userJoined", AddUser);

            chatHub.start().then(function () {
                chatHub.invoke("joinLobby", name, lang, userId).then(function (sentId)
                {
                    localStorage.userId = sentId;
                    userId = sentId;

                    chatHub.invoke("getUsers").then(function (users)
                    {
                        users.forEach(function (user)
                        {
                            if (user.id != userId)
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