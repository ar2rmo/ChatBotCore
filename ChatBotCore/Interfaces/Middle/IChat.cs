using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BotCore
{
    public interface IChatSession
    {
        [JsonProperty]
        Guid ChatId { get; }
    }

    public interface IChatProcessor
    {
        void SetChatSession(IChatSession sess);
        IEnumerable<IOutgoingMessage> Init();
        IEnumerable<IOutgoingMessage> ProcessIncomingMessage(IIncomingMessage msg);
    }

    public interface IChatProcessorFactory
    {
        IChatProcessor CreateChatProcessor();
    }
}
