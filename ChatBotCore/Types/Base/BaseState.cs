using System;
using System.Linq;
using NLog;
using System.Reflection;
using Newtonsoft.Json;
using BotCore.Managers;

namespace BotCore.Types
{
    public class StateTransition<State> : IStateTransition<State> where State : struct
    {
        [JsonProperty]
        public IOutgoingMessage outMessage { get; set; }
        [JsonProperty]
        public State? newState { get; set; }
        [JsonProperty]
        public State? oldState { get; set; }
    }

    public class StateDynamicProcFactory<State,Bag> : IStateDynamicProcessorFactory<State> where State : struct where Bag : new ()
    {
        protected DynamicChatProcessorStates<State,Bag> proc;

        public StateDynamicProcFactory(DynamicChatProcessorStates<State,Bag> proc)
        {
            this.proc = proc;
        }

        public IStateProcessor<State> Create(object state)
        {
            var ass = typeof(State).Assembly;

            var tuples = ass.GetTypes().
                        Select(r => (type: r, attr: r.GetCustomAttribute<StateAttribute>(true)) ).
                        ToList();

            var type = tuples.FirstOrDefault(x => x.attr != null && x.attr.State == state.ToString()).type;

            BaseState<State,Bag> res = (BaseState<State,Bag>)Activator.CreateInstance(type);

            res.SetChatProcessor(proc);

            return res;
        }
    }

    public abstract class BaseState<State, Bag> : IStateProcessor<State> where State : struct where Bag : new()
    {
        protected bool CanBack;
        protected Logger Log;
        [JsonProperty]
        protected StateTransition<State> Transition = new StateTransition<State>();

        public BaseState()
        {
            CanBack = true;
        }

        protected void SetNewState(State state)
        {
            Transition.newState = state;
             if(CanBack) SetOldState();
        }
        private void SetOldState()
        {
            var ass = this.GetType().Assembly;

            var tuples = ass.GetTypes().
                        Select(r => (type: r, attr: r.GetCustomAttribute<StateAttribute>(true))).
                        ToList();

            var oldState = tuples.FirstOrDefault(x => x.attr != null && x.type == this.GetType()).attr.State;

            Chat.OldState = (State)Enum.Parse(typeof(State), oldState);
        }


        public IStateTransition<State> ProcessIncomingMessage(IIncomingMessage msg)
        {
            var m = ProcessTransition(msg);
            Transition.outMessage = m;
            return Transition;
        }

        public abstract IOutgoingMessage ProcessTransition(IIncomingMessage msg);

        public abstract IStateTransition<State> OnInit();

        [JsonProperty]
        protected DynamicChatProcessorStates<State, Bag> Chat { get; private set; }

        public virtual void SetChatProcessor(DynamicChatProcessorStates<State,Bag> proc)
        {
            Chat = proc;
        }
    }
}