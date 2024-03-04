var profileApp = (function ()
{
    function LoadProfile()
    {
        langDictionary.GetOptions(document.getElementById("languageDDL"));

        var lang = "English";
        var name = "User";

        if (localStorage)
        {
            lang = localStorage.language;
            name = localStorage.userName;
        }

        $("#languageDDL").val(lang);
        $("#userNameTextBox").val(name);
    };
    function SaveProfile()
    {
        var lang = $("#languageDDL").val();
        var name = $("#userNameTextBox").val();

        if (localStorage)
        {
            localStorage.language = lang;
            localStorage.userName = name;
        }

        window.location = "/Home/Lobby";
    };

    return {
        LoadProfile: LoadProfile,
        SaveProfile: SaveProfile
    }

})();

profileApp.LoadProfile();