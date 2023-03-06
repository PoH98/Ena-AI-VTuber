using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3;
using Google.Cloud.TextToSpeech.V1;
using Newtonsoft.Json;

namespace AIVTuberView.Service
{
    internal class ConfigService
    {

        public static ConfigSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (!File.Exists("config.json"))
                    {
                        File.WriteAllText("config.json", JsonConvert.SerializeObject(new ConfigSettings()));
                    }
                    GoogleWebAuthorizationBroker.Folder = "AIVTuber";
                    _instance = JsonConvert.DeserializeObject<ConfigSettings>(File.ReadAllText("config.json"));
                    using FileStream s = new("client-secrets.json", FileMode.Open, FileAccess.Read);
                    _instance.Credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(s).Secrets,
                        TextToSpeechClient.DefaultScopes.Concat(new[]{ YouTubeService.Scope.Youtube,
                            YouTubeService.Scope.YoutubeUpload,
                            YouTubeService.Scope.YoutubeForceSsl,
                            YouTubeService.Scope.Youtubepartner,
                            YouTubeService.Scope.YoutubepartnerChannelAudit,
                            YouTubeService.Scope.YoutubeChannelMembershipsCreator
                        }),
                        "user",
                        CancellationToken.None
                    ).GetAwaiter().GetResult();
                }
                return _instance;
            }
        }

        private static ConfigSettings _instance;
    }

    public class ConfigSettings
    {
        public ConfigSettings()
        {

        }

        public ChatGPTConfig ChatGPT { get; set; }

        [JsonIgnore]
        public UserCredential Credential { get; set; }
        public string AIStory { get; set; }
    }

    public class ChatGPTConfig
    {
        public string Token { get; set; }
    }
}
