using Api.Managers;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;

namespace BotCore
{
    public class StateChatProcessor<State> : IChatProcessor where State : struct
    {
        [JsonIgnore]
        private State initState;

        Logger Log;

        public StateChatProcessor(State initState)
        {
            Log = new ApiLogManager().GetManager<StateChatProcessor<State>>();
            Log.Debug("Init state " + initState);
            this.initState = initState;
        }

        public IEnumerable<IOutgoingMessage> Init()
        {
            return ChangeState(initState);
        }

        private Dictionary<State, IStateProcessorFactory<State>> factories = new Dictionary<State, IStateProcessorFactory<State>>();
        private Dictionary<State, IStateProcessor<State>> procs = new Dictionary<State, IStateProcessor<State>>();

        [JsonProperty]
        private State state;

        private IEnumerable<IOutgoingMessage> ChangeState(State newState)
        {
            state = newState;
            Log.Debug("Change state " + state);
            var sp = GetStateProcessor(state);
            var tran = sp.OnInit();
            if (tran == null) yield break;
            if (tran.outMessage != null) yield return tran.outMessage;
            if (tran.newState.HasValue)
            {
                var m = ChangeState(tran.newState.Value);
                if (m != null) foreach (var mi in m) yield return mi;
            }
        }

        public void RegisterStateProcessorFactory(State state, IStateProcessorFactory<State> factory)
        {
            Log.Debug($"RegisterStateProcessorFactory => {factory}  with state {state}");
            factories.Add(state, factory);
        }

        public void RegisterStateProcessor(State state, IStateProcessor<State> proc)
        { 
            Log.Debug($"RegisterStateProcessor => {proc}  with state {state}");
            procs.Add(state, proc);
        }

        protected IStateProcessor<State> GetStateProcessor(State state)
        {
            if (procs.ContainsKey(state)) return procs[state];
            if (factories.ContainsKey(state)) return factories[state].Create();

            Log.Error($"There is no Processor or Factory for the State {state}");
            throw new Exception($"There is no Processor or Factory for the State {state}");
        }

        public IEnumerable<IOutgoingMessage> ProcessIncomingMessage(IIncomingMessage msg)
        {
            Log.Debug($"ProcessIncomingMessage");
            var sp = GetStateProcessor(state);
            var tran = sp.ProcessIncomingMessage(msg);
            if (tran == null) yield break;
            if (tran.outMessage != null) yield return tran.outMessage;
            if (tran.newState.HasValue)
            {
                var m = ChangeState(tran.newState.Value);
                if (m != null) foreach (var mi in m) yield return mi;
            }
        }

        public void SetChatSession(IChatSession sess)
        {
           
        }

        public override string ToString()
        {
            return $"StateChatProcessor :{{State\"{state}\", InitState :\"{initState}\"}}";
        }
    }

    public class DynamicChatProcessor<State> : IChatProcessor where State : struct
    {
        [JsonIgnore]
        private State initState;

        Logger Log;

        public DynamicChatProcessor()
        {
            Log = new ApiLogManager().GetManager<DynamicChatProcessor<State>>();
        }

        public DynamicChatProcessor(State initState)
        {
            Log = new ApiLogManager().GetManager<DynamicChatProcessor<State>>();
            Log.Debug("Init state " + initState);
            this.initState = initState;
        }

         public IEnumerable<IOutgoingMessage> Init()
         {
            return ChangeState(initState);
         }

        [JsonProperty]
        private IStateDynamicProcessorFactory<State> _factory;
        [JsonProperty]
        private Dictionary<State, IStateProcessor<State>> procs = new Dictionary<State, IStateProcessor<State>>();

        [JsonProperty]
        private State state;

        private IEnumerable<IOutgoingMessage> ChangeState(State newState)
        {
            state = newState;
            Log.Debug("Change state " + state);
            var sp = GetStateProcessor(state);
            var tran = sp.OnInit();
            if (tran == null) yield break;
            if (tran.outMessage != null) yield return tran.outMessage;
            if (tran.newState.HasValue)
            {
                var m = ChangeState(tran.newState.Value);
                if (m != null) foreach (var mi in m) yield return mi;
            }
        }

        public void RegisterStateProcessorFactory(IStateDynamicProcessorFactory<State> factory)
        {
            Log.Debug($"RegisterStateProcessorFactory => {factory}  with state {state}");
            _factory = factory;
        }

        public void RegisterStateProcessor(State state, IStateProcessor<State> proc)
        {
            Log.Debug($"RegisterStateProcessor => {proc}  with state {state}");
            procs.Add(state, proc);
        }

        protected IStateProcessor<State> GetStateProcessor(State state)
        {
            if (_factory != null) return _factory.Create(state);
            Log.Error($"There is no Processor or Factory for the State {state}");
            throw new Exception($"There is no Processor or Factory for the State {state}");
        }

        public IEnumerable<IOutgoingMessage> ProcessIncomingMessage(IIncomingMessage msg)
        {
            Log.Debug($"ProcessIncomingMessage");
            var sp = GetStateProcessor(state);
            var tran = sp.ProcessIncomingMessage(msg);
            if (tran == null) yield break;
            if (tran.outMessage != null) yield return tran.outMessage;
            if (tran.newState.HasValue)
            {
                var m = ChangeState(tran.newState.Value);
                if (m != null) foreach (var mi in m) yield return mi;
            }
        }

        public void SetChatSession(IChatSession sess)
        {

        }

        public override string ToString()
        {
            return $"StateChatProcessor :{{State\"{state}\", InitState :\"{initState}\"}}";
        }
    }
}
