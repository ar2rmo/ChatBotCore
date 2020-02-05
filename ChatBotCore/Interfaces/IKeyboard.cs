using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BotCore
{
    public interface IButton
    {
        [JsonProperty]
        String Key { get; }
        [JsonProperty]
        String Text { get; }
    }

    public interface IKeyboard
    {
        [JsonProperty]
        String Key { get; }
        [JsonProperty]
        Int32 ColNum { get; }
        [JsonProperty]
        IEnumerable<IButton> Buttons { get; }
    }
}
