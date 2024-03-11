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
                "code": "t-IT",
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
        const selectedLanguage = ref(localStorage && localStorage.language ? localStorage.language : languages[0].code);

        function saveProfile() {
            var lang = selectedLanguage.value;
            var name = userName.value;

            if (localStorage) {
                localStorage.language = lang;
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
            selectedLanguage,
            userName
        }
    }
}).mount('#profileDiv')