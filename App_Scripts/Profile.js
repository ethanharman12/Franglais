var profileApp = (function ()
{
    function LoadProfile()
    {
        var lang = "English";
        var name = "User";

        if (localStorage)
        {
            lang = localStorage.language;
            name = localStorage.userName;
        }

        $("#languageDDL :selected").text(lang);
        $("#userNameTextBox").val(name);
    };
    function SaveProfile()
    {
        var lang = $("#languageDDL :selected").text();
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