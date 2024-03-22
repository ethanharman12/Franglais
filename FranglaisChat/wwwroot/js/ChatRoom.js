const { createApp, ref } = Vue

createApp({
    setup() {
        var chatHub = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
        var roomId = 0;
        var userId = "";
        var soundsOn = ref(false);
        var speaking = ref(false);
        const users = ref([]);
        const messages = ref([]);

        function addUser(user) {
            users.value.push(user);
        };

        function disconnectUser(conn) {
            _.remove(users.value, user => user.id == conn);
        };

        function displayLanguage(language) {
            return langDictionary.Languages[language];
        };
        
        function receiveMessage(mess) {
            var message = mess.message;

            if (mess.sender.id != userId) {
                messClass = "theirMessage";
                if (mess.translation) {
                    message += "<br/>" + mess.translation;
                }

                if (soundsOn.value) {
                    playSound(mess);
                }
            }

            messages.value.push({
                label: mess.sender.userName,
                text: mess.message,
                translation: mess.translation,
                original: mess.original,
                mine: mess.sender.id == userId
            })

            //var labelCell = '<td class="' + messClass + 'Label">' + label + '</td>';
            //var messCell = '<td class="' + messClass + '">' + message + '</td>';
            //var row = (mess.sender.id != userId)
            //                ? labelCell + messCell + "<td></td>"
            //                : "<td></td>" + messCell + labelCell;

            //$("#chatWindow table").append('<tr class="row">' + row + '</tr>');
            //$("#chatWindow").append("<div class='" + messClass + "Label col-md-3'>" + label + "</div>" +
            //    "<div class='message " + messClass + " col-md-8'>" + message + "</div>");

            //$("#chatWindow").append("<div class='message " + messClass + "'>" +
            //    "<div class='" + messClass + "Label>" + label + "</div><span>" + message + "</span></div>");

            $('#chatWindow').animate({ scrollTop: $('#chatWindow').prop("scrollHeight") }, 500);
        };

        function playSound(message) {
            if (typeof (SpeechSynthesisUtterance) != "undefined") {
                var msg = null;
                if (message.translation) {
                    msg = new SpeechSynthesisUtterance(message.translation);
                }
                else {
                    msg = new SpeechSynthesisUtterance(message.message);
                }

                msg.lang = localStorage.chatLanguage;

                window.speechSynthesis.speak(msg);
            }
        }

        function sendMessage() {
            var mess = $("#messageBox").val();
            var sendTime = new Date();
            var messObj = { Message: mess, ClientSent: sendTime };
            chatHub.invoke("sendMessage", roomId, messObj);

            $("#messageBox").val("").focus();
        };

        function setUpHub() {
            roomId = parseInt(document.getElementById('roomId').value);

            chatHub.on("receiveMessage", receiveMessage);
            chatHub.on("userDisconnected", disconnectUser);
            chatHub.on("userJoined", addUser);

            if (localStorage) {
                userId = localStorage.userId;
            }

            chatHub.start().then(function () {
                chatHub.invoke("joinRoom", roomId, userId);
            });

        };

        function speak() {
            if (typeof (webkitSpeechRecognition) != "undefined") {
                var recognition = new webkitSpeechRecognition();

                recognition.lang = localStorage.chatLanguage;

                recognition.onresult = function (event) {
                    if (event.results.length > 0) {
                        $("#messageBox").val(event.results[0][0].transcript);
                    }
                };

                recognition.onerror = function (event) {
                    alert("Speech to Text error: " + event.error);
                    speaking.value = false;
                };

                recognition.onend = function () {
                    speaking.value = false;
                };

                recognition.start();

                speaking.value = true;
            }
        };

        function toggleSound() {
            soundsOn.value = !soundsOn.value;
        };

        setUpHub();

        return {
            displayLanguage,
            messages,
            sendMessage,
            speak,
            speaking,
            soundsOn,
            toggleSound,
            users
        };
    }
}).mount('#chatApp')