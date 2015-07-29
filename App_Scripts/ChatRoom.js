var chatRoomApp = (function ()
{
    var chatHub = $.connection.chatHub;
    var roomId = 0;
    var userId = "";
    var soundsOn = false;

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
            var html = '<div id="User' + user.Id + '" class="col-md-2 participantDiv">' +
                        user.UserName + ' - ' + langDictionary.Languages[user.Language] +
                       '</div>';
            $("#participantsDiv").append(html);
        }
    };
    function DisconnectUser(conn)
    {
        $("#User" + conn).remove();
    };
    function ReceiveMessage(mess)
    {
        var messClass = "myMessage";
        var message = mess.Message;
        var label = mess.Sender.UserName;

        if (mess.Sender.Id != userId)
        {
            messClass = "theirMessage";
            if (mess.Translation)
            {
                message += "<br/>" + mess.Translation;
            }

            if (soundsOn)
            {
                PlaySound(mess);
            }
        }

        var labelCell = '<td class="' + messClass + 'Label">' + label + '</td>';
        var messCell = '<td class="' + messClass + '">' + message + '</td>';
        var row = (mess.Sender.Id != userId)
                        ? labelCell + messCell + "<td></td>"
                        : "<td></td>" + messCell + labelCell;

        $("#chatWindow table").append('<tr class="row">' + row + '</tr>');
        //$("#chatWindow table").append("<div class='" + messClass + "Label'>" + label + "</div>" +
        //                        "<div class='message " + messClass + "'>" + message + "</div>");

        $('#chatWindow').animate({ scrollTop: $('#chatWindow').prop("scrollHeight") }, 500);
    };

    function PlaySound(message)
    {
        if (typeof (SpeechSynthesisUtterance) != "undefined")
        {
            var msg = null;
            if (message.Translation)
            {
                msg = new SpeechSynthesisUtterance(message.Translation);
            }
            else
            {
                msg = new SpeechSynthesisUtterance(message.Message);
            }

            msg.lang = localStorage.language;

            window.speechSynthesis.speak(msg);
        }
    }
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
    function Speak()
    {
        if (typeof (webkitSpeechRecognition) != "undefined")
        {
            var recognition = new webkitSpeechRecognition();

            recognition.lang = localStorage.language;

            recognition.onresult = function (event)
            {
                if (event.results.length > 0)
                {
                    $("#messageBox").val(event.results[0][0].transcript);
                }
            };

            recognition.onerror = function (event)
            {
                alert("Speech to Text error: " + event.error);
            };

            recognition.onend = function ()
            {
                $("#micSpan").toggleClass("glyphicon-bullhorn glyphicon-headphones");
            };

            recognition.start();

            $("#micSpan").toggleClass("glyphicon-bullhorn glyphicon-headphones");
        }
    };
    function ToggleSound()
    {
        soundsOn = !soundsOn;
        $("#soundSpan").toggleClass("glyphicon-volume-off glyphicon-volume-up");
    };

    return {
        PlaySound: PlaySound,
        SendMessage: SendMessage,
        SetUpHub: SetUpHub,
        Speak: Speak,
        ToggleSound: ToggleSound
    };
})();

chatRoomApp.SetUpHub();