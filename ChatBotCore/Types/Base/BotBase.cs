using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using Api.Managers;
using Newtonsoft.Json;
using BotCore.Managers;

namespace BotCore
{
    public abstract class BotBase : IBot
    {
        [JsonProperty]
        protected IConfiguration _conf;
        [JsonIgnore]
        protected Logger Log;
        [JsonProperty]
        protected List<Type> _chat_procs_types;
        [JsonIgnore]
        protected ChatManager _chat_manager = new ChatManager();
        [JsonProperty]
        protected List<IMessageProcessor> _procs = new List<IMessageProcessor>();
        [JsonProperty]
        protected List<IChatProcessorFactory> _chat_procs_factorys = new List<IChatProcessorFactory>();
        public BotBase()
        {
            Log = new BotLogManager().GetManager<BotBase>();
        }

        public BotBase(IConfiguration configuration)
        {
            Log = new BotLogManager().GetManager<BotBase>();
            Log.Info($"Construct BotBase with configure {configuration}");
            _conf = configuration;
        }

        public void RegisterChatProcessor<T>() where T : IChatProcessor, new()
        {
            Log.Debug($"RegisterChatProcessor => {typeof(T)}");
            if (_chat_procs_types == null) _chat_procs_types = new List<Type>();
            _chat_procs_types.Add(typeof(T));
        }

        public void RegisterChatProcessorFactory(IChatProcessorFactory factory)
        {
            Log.Debug($"RegisterChatProcessorFactory => {factory}");
            _chat_procs_factorys.Add(factory);
        }

        public void RegDymanicChatProcFactory(IChatProcessorFactory factory)
        {
            Log.Debug($"RegisterChatProcessorFactory => {factory}");
            _chat_procs_factorys.Add(factory);
        }

        public void RegisterMessageProcessor(IMessageProcessor proc)
        {
            Log.Debug($"RegisterMessageProcessor => {proc}");
            if (_procs == null) _procs = new List<IMessageProcessor>();
            _procs.Add(proc);
        }

        public abstract Task SendMessageAsync(IChatSession sess, IOutgoingMessage msg);

        public abstract void Start();

        public abstract void Stop();

        protected async Task ProcessIncomingMessageAsync(IIncomingMessage incoming, IChatSession session)
        {
            Log.Debug($"ProcessIncomingMessageAsync with inc msg {incoming} and sess {session}");
            if (_procs != null)
            {
                Log.Trace($"Use _procs:{_procs} road");
                foreach (var p in _procs)
                {
                    var mo = p.ProcessIncomingMessage(session, incoming);
                    
                    foreach (var msg in mo)
                    {
                        await SendMessageAsync(session, msg);
                    }
                }
            }

            var chat = _chat_manager.GetChat(session.ChatId);
            if (chat == null)
            {
                if (_chat_procs_types != null)
                {
                    Log.Trace($"Use _chat_procs_types:{_chat_procs_types} road");
                    foreach (var t in _chat_procs_types)
                    {
                        var i = Activator.CreateInstance(t) as IChatProcessor;
                        i.SetChatSession(session);
                        var mis = i.Init();
                        foreach (var msg in mis)
                        {
                            await SendMessageAsync(session, msg);
                        }
                        chat = i;
                    }
                }
                if (_chat_procs_factorys != null)
                {
                    Log.Trace($"Use _chat_procs_factorys:{_chat_procs_factorys} road");
                    foreach (var fac in _chat_procs_factorys)
                    {
                        var i = fac.CreateChatProcessor();
                        i.SetChatSession(session);
                        var mos = i.Init();
                        foreach (var msg in mos)
                        {
                            await SendMessageAsync(session, msg);
                        }
                        chat = i;
                    }
                }
            }
            bool isWait = false;
            Log.Trace($"Use _chat_procs:{chat} road");
            var oms = chat.ProcessIncomingMessage(incoming);
            foreach (var msg in oms)
            {
                await SendMessageAsync(session, msg);
                if (msg.Type == Types.Enums.MsgOutType.Wait) isWait = true;
            }

            if(!isWait)
                _chat_manager.SaveChat(session.ChatId, chat);
        }

        public override string ToString()
        {
            return $"BotBase :{{\"{_conf}\", " +
                //$"proc \"{_procs}\", " +
                $"chat_mngr :\"{_chat_manager}\", " +
                $"chat_procs_factorys :\"{_chat_procs_factorys}\"}}";
        }
    }
}