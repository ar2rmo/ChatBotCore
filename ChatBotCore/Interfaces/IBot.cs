using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BotCore
{
    public interface IConfiguration
    {
        [JsonProperty]
        String Token { get; }
    }
    

    public interface IBot
    {
        Task SendMessageAsync(IChatSession sess, IOutgoingMessage msg);
        Task ProcessIncomingMessageAsync(String json, IMessageProcessor proc);
    }
}
