using LiteDB;
using OpenAI.GPT3.ObjectModels.RequestModels;
using System.Security.Cryptography;
using System.Text;

namespace AIVTuberView.Service
{
    public class AIBrain : IDisposable
    {
        private LiteDatabase db;
        private ILiteCollection<ChatMessageRecord> collection;
        public string BrainPath = "Brain";
        public AIBrain(string userName)
        {
            if (!Directory.Exists(BrainPath))
            {
                Directory.CreateDirectory(BrainPath);
            }
            db = new LiteDatabase(Path.Join(BrainPath, GetStringSha256Hash(userName) + ".db"));
            collection = db.GetCollection<ChatMessageRecord>("chatHistory");
        }

        public List<ChatMessage> GetChats()
        {
            return collection.FindAll().OrderByDescending(x => x.DateTime).Take(20).Select(x => x.ChatMessage).ToList();
        }

        public void Add(ChatMessage message)
        {
            if(Count() > 20)
            {
                collection.DeleteAll();
            }
            collection.Insert(new ChatMessageRecord
            {
                ChatMessage = message,
                DateTime = DateTime.Now
            });
        }

        public long Count() { return collection.Count(); }

        public void Dispose()
        {
            db.Checkpoint();
        }

        string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = SHA256.Create())
            {
                byte[] textData = Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        private class ChatMessageRecord
        {
            public DateTime DateTime { get; set; }
            public ChatMessage ChatMessage { get; set; }
        }
    }
}
