using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;

namespace FranglaisChat
{
    public class ChatBot : IChatBot
    {
        private readonly string _apiKey;
        private readonly RestClient _client;

        private List<ChatGPTMessage> messageHistory = new List<ChatGPTMessage>();

        public ChatBot(IConfiguration config)
        {
            _apiKey = config["Franglais:ChatGPTKey"];
            _client = new RestClient("https://api.openai.com/v1/chat/completions");

            messageHistory.Add(new ChatGPTMessage("system", "You are a friend chatting with a new student to the language."));
        }

        public string SendMessage(string message)
        {
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Bearer {_apiKey}");

            messageHistory.Add(new ChatGPTMessage("user", message));

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = messageHistory
            };

            request.AddJsonBody(JsonConvert.SerializeObject(requestBody));

            var response = _client.Execute(request);

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content ?? string.Empty);

            var responseMessage = jsonResponse?.choices[0]?.message?.content?.ToString()?.Trim() ?? string.Empty;

            messageHistory.Add(new ChatGPTMessage("assistant", responseMessage));

            return responseMessage;
        }
    }

    public class ChatGPTMessage
    {
        public string role { get; set; }
        public string content { get; set; }

        public ChatGPTMessage(string Role, string Content)
        {
            role = Role;
            content = Content;
        }
    }
}
