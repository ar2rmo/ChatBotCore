using Newtonsoft.Json;
using System;

namespace BotCore.Types.Base
{
    public class Configure : IConfiguration
    {
        [JsonProperty]
        public String Token { get; set; }

        [JsonProperty]
        public String WebHook { get; set; }

        [JsonProperty]
        public String UriListener { get; set; }

        public override string ToString()
        {
            return $"Configure :{{Token\"{Token}\", " +
                $"WebHook :\"{WebHook}\", ";
        }
    }
}
