using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.ResponseModels;

namespace AIVTuberView.Service
{
    internal class ChatGPTService
    {
        private readonly OpenAIService ChatGpt;
        private string Topic;
        public ChatGPTService()
        {
            ChatGpt = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = ConfigService.Instance.ChatGPT.Token
            });
        }
        public async Task<string> GetResponse(string message, string userName)
        {
            using (AIBrain brain = new AIBrain(userName))
            {
                if (!brain.GetChats().Any())
                {
                    brain.Add(ChatMessage.FromSystem(ConfigService.Instance.AIStory));
                }
                brain.Add(ChatMessage.FromUser(message));
                var result = brain.GetChats();
                result.Reverse();
                ChatCompletionCreateResponse response = await ChatGpt.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
                {
                    Model = Models.ChatGpt3_5Turbo,
                    Messages = result,
                    User = userName,
                    MaxTokens = 256,
                    Temperature = 1
                });
                try
                {
                    Console.WriteLine("Token used: " + response.Usage.TotalTokens);
                    brain.Add(response.Choices.First().Message);
                    return response.Choices.First().Message.Content;
                }
                catch (ArgumentNullException)
                {
                    response = await ChatGpt.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
                    {
                        Model = Models.ChatGpt3_5Turbo,
                        Messages = new List<ChatMessage> {
                        ChatMessage.FromSystem(ConfigService.Instance.AIStory),
                        ChatMessage.FromUser("Meow!")
                    },
                        User = userName,
                        MaxTokens = 256,
                        Temperature = 1
                    });
                    Console.WriteLine("Token used: " + response.Usage.TotalTokens);
                    brain.Add(response.Choices.First().Message);
                    return response.Choices.First().Message.Content;
                }
            }

        }

        public async Task<string> CreateTalk()
        {
            ChatCompletionCreateResponse response = await ChatGpt.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
            {
                Model = Models.ChatGpt3_5Turbo,
                Messages = new List<ChatMessage> { ChatMessage.FromSystem(ConfigService.Instance.AIStory), ChatMessage.FromUser("Say something interesting.") },
                MaxTokens = 256,
                Temperature = 1
            });
            Console.WriteLine("Token used: " + response.Usage.TotalTokens);
            return response.Choices.First().Message.Content;
        }

        public async Task<string> GetTitle()
        {
            ChatCompletionCreateResponse response = await ChatGpt.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
            {
                Model = Models.ChatGpt3_5Turbo,
                Messages = new List<ChatMessage>()
                {
                    ChatMessage.FromUser("Create a Youtube Live Stream Topic. Title only"),
                },
                MaxTokens = 32
            });
            Topic = response.Choices.First().Message.Content.Replace("\n", "").Replace("\r", "");
            return response.Choices.First().Message.Content.Replace("\"", "").Replace("\n", "").Replace("\r", "");
        }
    }
}
