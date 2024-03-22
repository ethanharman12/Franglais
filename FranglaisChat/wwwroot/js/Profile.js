const { createApp, ref } = Vue

createApp({
    setup() {
        const languages = [
            {
                "code": "en-US",
                "name": "English (US)"
            },
            {
                "code": "en-GB",
                "name": "English (UK)"
            },
            {
                "code": "fr-FR",
                "name": "French (France)"
            },
            {
                "code": "fr-CA",
                "name": "French (Canada)"
            },
            {
                "code": "es-MX",
                "name": "Spanish (Mexico)"
            },
            {
                "code": "es-ES",
                "name": "Spanish (Spain)"
            },
            {
                "code": "de-DE",
                "name": "German"
            },
            {
                "code": "it-IT",
                "name": "Italian"
            },
            {
                "code": "ru-RU",
                "name": "Russian"
            },
            {
                "code": "ja-JP",
                "name": "Japanese"
            }];

        const userName = ref(localStorage && localStorage.userName ? localStorage.userName : "");
        const selectedNativeLanguage = ref(localStorage && localStorage.nativeLanguage ? localStorage.nativeLanguage : languages[0].code);
        const selectedChatLanguage = ref(localStorage && localStorage.chatLanguage ? localStorage.chatLanguage : languages[0].code);

        function saveProfile() {
            var chatLang = selectedChatLanguage.value;
            var nativeLang = selectedNativeLanguage.value;
            var name = userName.value;

            if (localStorage) {
                localStorage.chatLanguage = chatLang;
                localStorage.nativeLanguage = nativeLang;
                localStorage.userName = name;
            }
            else {
                //?
            }

            window.location = "/Home/Lobby";
        }

        return {
            languages,
            saveProfile,
            selectedChatLanguage,
            selectedNativeLanguage,
            userName
        }
    }
}).mount('#profileDiv')