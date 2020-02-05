using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BotCore.Managers;

namespace BotCore
{
    public abstract class BotBase : IBot
    {
        [JsonProperty]
        protected IConfiguration _conf;
        [JsonProperty]
        protected List<Type> _chat_procs_types;
        [JsonProperty]
        protected List<IMessageProcessor> _procs = new List<IMessageProcessor>();
        public BotBase()
        {
            
        }

        public BotBase(IConfiguration configuration)
        {
            _conf = configuration;
        }

        public void RegisterMessageProcessor(IMessageProcessor proc)
        {
            if (_procs == null) _procs = new List<IMessageProcessor>();
            _procs.Add(proc);
        }

        public abstract Task SendMessageAsync(IChatSession sess, IOutgoingMessage msg);

        public abstract void Start();

        public abstract void Stop();

        protected async Task ProcessIncomingMessageAsync(IIncomingMessage incoming, IChatSession session)
        {
            if (_procs != null)
            {
                foreach (var p in _procs)
                {
                    var mo = p.ProcessIncomingMessage(session, incoming);
                    
                    foreach (var msg in mo)
                    {
                        await SendMessageAsync(session, msg);
                    }
                }
            }
        }
    }
}