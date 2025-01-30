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

        public GoogleChatBot(IConfiguration config)
        {
            _apiKey = config["Franglais:GoogleAIKey"];
            _client = new RestClient($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}");

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
                system_instruction = new GoogleAIMessage("", "You are a friend chatting with a beginner student of the language. Speak in the language they are using."),
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
