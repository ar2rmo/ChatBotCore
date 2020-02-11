using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BotCore
{
    public abstract class BotBase : IBot
    {
        protected IConfiguration conf;
        public BotBase(IConfiguration configuration)
        {
            conf = configuration;
        }

        public abstract Task SendMessageAsync(IChatSession sess, IOutgoingMessage msg);

        protected async Task ProcessIncomingMessageAsync(IMessageProcessor proc, IIncomingMessage msg, IChatSession sess)
        {
            var ms = proc.ProcessIncomingMessage(sess, msg);
            foreach (var m in ms)
            {
                await SendMessageAsync(sess, m);
            }
        }

        protected abstract bool ParseIncomingMessage(String json, out IChatSession sess, out IIncomingMessage msg);

        public async Task ProcessIncomingMessageAsync(String json, IMessageProcessor proc)
        {
            if (ParseIncomingMessage(json, out var sess, out var msg))
            {
                await ProcessIncomingMessageAsync(proc, msg, sess);
            }
        }
    }
}