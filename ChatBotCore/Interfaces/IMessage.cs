using BotCore.Types.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BotCore
{
    public interface IMessage
    {
        [JsonProperty]
        String Text { get; }
    }

    public interface IOutgoingMessage : IMessage
    {
        [JsonProperty]
        IKeyboard Keyboard { get; }
        [JsonProperty]
        MsgOutType Type { get; }
    }

    public interface IIncomingMessage : IMessage
    {
        [JsonProperty]
        MessageInType Type { get; }
        [JsonProperty]
        User User { get; }
        [JsonProperty]
        Location Location { get; }
    }

    public interface IMessageProcessor
    {
        IEnumerable<IOutgoingMessage> ProcessIncomingMessage(IChatSession sess, IIncomingMessage msg);
    }
}
