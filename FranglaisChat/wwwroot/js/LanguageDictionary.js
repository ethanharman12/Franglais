var langDictionary = (function ()
{
    var dictionary = [];

    dictionary["en-US"] = "English (US)";
    dictionary["en-GB"] = "English (UK)";    
    dictionary["fr-FR"] = "French (France)";
    dictionary["fr-CA"] = "French (Canada)";
    dictionary["es-MX"] = "Spanish (Mexico)";
    dictionary["es-ES"] = "Spanish (Spain)";
    dictionary["de-DE"] = "German";
    dictionary["it-IT"] = "Italian";
    dictionary["ru-RU"] = "Russian";
    dictionary["ja-JP"] = "Japanese";
    //dictionary[""] = "";

    function GetDDLOptions(ddl)
    {
        Object.keys(dictionary).forEach(function (key)
        {
            var option = document.createElement("option");

            option.text = dictionary[key];
            option.value = key;

            ddl.appendChild(option);
        });
    };

    return {
        GetOptions: GetDDLOptions,
        Languages: dictionary
    }
})();