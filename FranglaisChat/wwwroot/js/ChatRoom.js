var chatRoomApp = (function ()
{
    var chatHub = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
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
        var userEle = document.getElementById("User" + user.id);
        if (!userEle)
        {
            var html = '<div id="User' + user.id + '" class="participantDiv"><span class="userName">' +
                        user.userName + '</span> - <span class="userLanguage">' + langDictionary.Languages[user.language] +
                       '</span></div>';
            $("#participantsDiv").append(html);
        }
    };
    function DisconnectUser(userId)
    {
        $("#User" + userId).remove();
    };
    function ReceiveMessage(mess)
    {
        var messClass = "myMessage";
        var message = mess.message;
        var label = mess.sender.userName;

        if (mess.sender.id != userId)
        {
            messClass = "theirMessage";
            if (mess.translation)
            {
                message += "<br/>" + mess.translation;
            }

            if (soundsOn)
            {
                PlaySound(mess);
            }
        }

        //var labelCell = '<td class="' + messClass + 'Label">' + label + '</td>';
        //var messCell = '<td class="' + messClass + '">' + message + '</td>';
        //var row = (mess.sender.id != userId)
        //                ? labelCell + messCell + "<td></td>"
        //                : "<td></td>" + messCell + labelCell;

        //$("#chatWindow table").append('<tr class="row">' + row + '</tr>');
        $("#chatWindow").append("<div class='" + messClass + "Label col-md-3'>" + label + "</div>" +
                                "<div class='message " + messClass + " col-md-8'>" + message + "</div>");

        //$("#chatWindow").append("<div class='message " + messClass + "'>" +
        //    "<div class='" + messClass + "Label>" + label + "</div><span>" + message + "</span></div>");

        $('#chatWindow').animate({ scrollTop: $('#chatWindow').prop("scrollHeight") }, 500);
    };

    function PlaySound(message)
    {
        if (typeof (SpeechSynthesisUtterance) != "undefined")
        {
            var msg = null;
            if (message.translation)
            {
                msg = new SpeechSynthesisUtterance(message.translation);
            }
            else
            {
                msg = new SpeechSynthesisUtterance(message.message);
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
        chatHub.invoke("sendMessage", roomId, messObj);

        $("#messageBox").val("").focus();
    };
    function SetUpHub()
    {
        roomId = parseInt(document.getElementById('roomId').value);

        chatHub.on("receiveMessage", ReceiveMessage);
        chatHub.on("userDisconnected", DisconnectUser);
        chatHub.on("userJoined", AddUser);

        if (localStorage)
        {
            userId = localStorage.userId;
        }

        chatHub.start().then(function () {
            chatHub.invoke("joinRoom", roomId, userId);
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
                $("#micSpan").toggleClass("bi-mic bi-mic-fill");
            };

            recognition.start();

            $("#micSpan").toggleClass("bi-mic bi-mic-fill");
        }
    };
    function ToggleSound()
    {
        soundsOn = !soundsOn;
        $("#soundSpan").toggleClass("bi-volume-off bi-volume-up");
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