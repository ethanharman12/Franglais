using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace FranglaisChat
{
    public class GoogleChatBot : IChatBot
    {
        private readonly string _apiKey;
        private readonly RestClient _client;

        private List<GoogleAIMessage> messageHistory = new List<GoogleAIMessage>();

        private string botPrompt;

        private readonly string friendBotPrompt = "You are a friend chatting with a beginner student of the language. Speak only in the language they are using.";
        private readonly string serverBotPrompt = "You are a waiter in a restaurant chatting with a beginner student of the language. Speak only in the language they are using. Be formal and polite.";
        private readonly string loveInterestBotPrompt = "You are a love interest chatting with a beginner student of the language. Speak only in the language they are using. Flirt casually.";

        public GoogleChatBot(IConfiguration config)
        {
            _apiKey = config["Franglais:GoogleAIKey"];
            _client = new RestClient($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}");

            botPrompt = friendBotPrompt;
            //messageHistory.Add(new GoogleAIMessage("system_instruction", "You are a friend chatting with a new student to the language."));
        }

        public string SendMessage(string message)
        {
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            //request.AddHeader("Authorization", $"Bearer {_apiKey}");

            messageHistory.Add(new GoogleAIMessage("user", message));

            var requestBody = new
            {
                //model = "gpt-3.5-turbo",
                system_instruction = new GoogleAIMessage("", botPrompt),
                //Try to point out times when they are using words or phrases incorrectly, but don't be too critical. Try to keep your responses short and use simple vocabulary.
                contents = messageHistory
            };

            request.AddJsonBody(JsonConvert.SerializeObject(requestBody));

            try
            {
                var response = _client.Execute(request);

                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content ?? string.Empty);

                var responseMessage = jsonResponse?.candidates[0]?.content?.parts[0]?.text?.ToString()?.Trim() ?? string.Empty;

                messageHistory.Add(new GoogleAIMessage("model", responseMessage));
                
                return responseMessage;
            }
            catch (Exception ex)
            {
                //log ex               
                return "An error has occurred when getting the response from the ChatBot.";
            }            
        }

        public void SetMode(BotModeEnum botMode)
        {
            switch (botMode)
            {
                case BotModeEnum.Friend: botPrompt = friendBotPrompt;
                    break;
                case BotModeEnum.Server: botPrompt = serverBotPrompt;
                    break;
                case BotModeEnum.LoveInterest: botPrompt = loveInterestBotPrompt;
                    break;
            }
        }
    }

    public class GoogleAIMessage
    {
        public string role { get; set; }
        public Parts parts { get; set; }

        public GoogleAIMessage(string Role, string Content)
        {
            role = Role;
            parts = new Parts { text = Content };
        }
    }

    public class Parts
    {
        public string text { get; set; }
    }
}
