using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BotCore
{
    public interface IConfiguration
    {
        String Token { get; }
        String WebHook { get; }
    }
    

    public interface IBot
    {
        Task InitAsync();
        Task SendMessageAsync(IChatSession sess, IOutgoingMessage msg);
        Task ProcessIncomingMessageAsync(String json, IMessageProcessor proc);
    }
}
