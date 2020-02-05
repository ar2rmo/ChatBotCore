using Newtonsoft.Json;
using System;
using System.IO;

namespace BotCore.Managers
{
    public class FileManager
    {
        public static BotProcessorFM BotProcessor => BotProcessorFM.FileManager;

        public static ChatFM Chat => ChatFM.FileManager;

        public static MassageFM Massage => MassageFM.FileManager;

    }

    public class BotProcessorFM
    {
        private JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        string path = "Saves/Bot.log";
        private static readonly Lazy<BotProcessorFM> lazy = new Lazy<BotProcessorFM>(() => new BotProcessorFM());

        public static BotProcessorFM FileManager
        {
            get
            {
                return lazy.Value;
            }
        }

        private BotProcessorFM()
        {

        }

        public void SaveBotProcessor(BotProcessor obj)
        {
            var json = JsonConvert.SerializeObject(obj, settings);
            File.WriteAllText(path, json);
        }

        public BotProcessor LoadBotPocessor()
        {
            if (File.Exists("Saves"))
            {
                string json = File.ReadAllText(path);

                object res = JsonConvert.DeserializeObject<object>(json, settings);

                if (res is BotProcessor)
                    return res as BotProcessor;
            }
            return null;
        }
    }

    public class ChatFM
    {
        private JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        private string root = "Saves/Chats/";
        private static readonly Lazy<ChatFM> lazy = new Lazy<ChatFM>(() => new ChatFM());

        public static ChatFM FileManager
        {
            get
            {
                return lazy.Value;
            }
        }

        private ChatFM()
        {

        }

        private string GetPath(Guid key) => root + key + ".json";

        public void SaveChat(Guid key, IChatProcessor chat)
        {
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            var save = JsonConvert.SerializeObject(chat, settings);

            File.WriteAllText(GetPath(key), save);
        }

        public bool Contains(Guid key)
        {
            return File.Exists(GetPath(key));
        }

        public IChatProcessor GetChat(Guid key)
        {
            string json = File.ReadAllText(GetPath(key));

            var chat = JsonConvert.DeserializeObject<IChatProcessor>(json,settings);
            return chat;
        }

        public void DeleteChat(Guid guid)
        {
            File.Delete(GetPath(guid));
        }
    }

    public class MassageFM
    {
        private JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        private string root = "Saves/Chats/";
        private static readonly Lazy<MassageFM> lazy = new Lazy<MassageFM>(() => new MassageFM());

        public static MassageFM FileManager
        {
            get
            {
                return lazy.Value;
            }
        }

        private MassageFM()
        {

        }

        private string GetPath(Guid key) => root + "msg-" + key + ".json";

        public void SaveMsg(Guid key, IOutgoingMessage msg)
        {
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            var save = JsonConvert.SerializeObject(msg, settings);

            File.WriteAllText(GetPath(key), save);
        }

        public bool Contains(Guid key)
        {
            return File.Exists(GetPath(key));
        }

        public IOutgoingMessage GetMsg(Guid key)
        {
            string json = File.ReadAllText(GetPath(key));
            File.Delete(GetPath(key));
            var msg = JsonConvert.DeserializeObject<IOutgoingMessage>(json, settings);
            return msg;
        }

        public void DeleteMsg(Guid guid)
        {
            File.Delete(GetPath(guid));
        }
    }
}
