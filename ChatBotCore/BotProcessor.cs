using BotCore.Managers;
using Newtonsoft.Json;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace BotCore
{
    public class BotProcessor
    {
        [JsonProperty]
        private List<IBot> _bots;
        [JsonIgnore]
        private Logger Log = new BotLogManager().GetManager<BotProcessor>();

        public void Add(IBot bot)
        {
            if (!_bots.Contains(bot))
                _bots.Add(bot);
        }

        public void Remove(IBot bot)
        {
            if (_bots.Contains(bot))
                _bots.Remove(bot);
        }

        public BotProcessor()
        {
            _bots = new List<IBot>();
        }

        public BotProcessor(IEnumerable<IBot> bots)
        {
            _bots = bots.ToList();
        }

        public void On()
        {
            _bots.ForEach(x => x.Start());
        }

        public void Off()
        {
            _bots.ForEach(x => x.Stop());
            FileManager.BotProcessor.SaveBotProcessor(this);
        }
    }
}