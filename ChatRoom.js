var chatRoomApp = (function ()
{
    var chatHub = $.connection.chatHub;
    var roomId = 0;
    var connId = "";
    var users = [];

    //private
    function AddUser(user)
    {
        users.push(user);
        DisplayUser(user);
    };
    function DiplayUser(user)
    {
        var html = '<div id="User' + user.Id + '" class="userDiv">' +
                    user.UserName +
                   '</div>';
        $("#usersDiv").append("html");

        var userEle = document.getElementById("User" + user.Id);
        userEle.addEventListener("click", Invite);
    };
    function DisconnectUser(name)
    {
        $("#User" + name).fadeOut(2);
    };
    function ReceiveMessage(mess)//, sender)
    {
        var messClass = (mess.Sender.ConnectionId == connId) ? "myMessage" : "theirMessage";
        $("#chatWindow").append("<div class='message " + messClass + "'>"
                                + mess.Message + "<br/>" + mess.Translation +
                                "</div>");
    };

    function SendMessage()
    {
        var mess = $("#messageBox").val();
        var sendTime = new Date();
        var messObj = { Message: mess, ClientSent: sendTime };
        chatHub.server.sendMessage(roomId, messObj);

        $("#messageBox").val("");
        //ReceiveMessage(messObj, "me");
    };
    function SetUpHub()
    {
        roomId = document.getElementById('roomId').textContent;

        chatHub.client.receiveMessage = ReceiveMessage;
        chatHub.client.userDisconnected = DisconnectUser;
        chatHub.client.userJoined = AddUser;

        $.connection.hub.start().done(function ()
        {
            if (localStorage)
            {
                connId = localStorage.connectionId;
            }

            chatHub.server.joinRoom(roomId, connId);

            if (localStorage)
            {
                connId = $.connection.hub.id;
                localStorage.connectionId = connId;                
            }
        });
    };

    return {
        SendMessage: SendMessage,
        SetUpHub: SetUpHub
    };
})();

chatRoomApp.SetUpHub();