var chatRoomApp = (function ()
{
    var chatHub = $.connection.chatHub;
    var roomId = 0;
    var userId = "";
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
    function DisconnectUser(userId)
    {
        $("#User" + userId).fadeOut(2);
    };
    function ReceiveMessage(mess)
    {
        var messClass = (mess.Sender.Id == userId) ? "myMessage" : "theirMessage";
        var message = (mess.Sender.Id == userId) ? mess.Message : mess.Sender.UserName + ": " + mess.Message;

        if (mess.Translation)
        {
            message += "<br/>" + mess.Translation;
        }

        $("#chatWindow").append("<div class='message " + messClass + "'>"
                                + message +
                                "</div>");

        $('#chatWindow').animate({ scrollTop: $('#chatWindow').prop("scrollHeight") }, 500);
    };

    function SendMessage()
    {
        var mess = $("#messageBox").val();
        var sendTime = new Date();
        var messObj = { Message: mess, ClientSent: sendTime };
        chatHub.server.sendMessage(roomId, messObj);

        $("#messageBox").val("").focus();
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
                userId = localStorage.userId;
            }

            chatHub.server.joinRoom(roomId, userId);
        });
    };

    return {
        SendMessage: SendMessage,
        SetUpHub: SetUpHub
    };
})();

chatRoomApp.SetUpHub();