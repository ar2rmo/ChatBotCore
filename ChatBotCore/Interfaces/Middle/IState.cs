using Newtonsoft.Json;

namespace BotCore
{
    public interface IStateTransition<State> where State : struct
    {
        [JsonProperty]
        IOutgoingMessage outMessage { get; }
        [JsonProperty]
        State? newState { get; }
        [JsonProperty]
        State? oldState { get; }
    }

    public interface IStateProcessorFactory<State> where State : struct
    {
        IStateProcessor<State> Create();
    }

    public interface IStateProcessor<State> where State : struct
    {
        IStateTransition<State> OnInit();
        IStateTransition<State> ProcessIncomingMessage(IIncomingMessage msg);
    }

    public interface IStateDynamicProcessorFactory<State> where State : struct
    {
        IStateProcessor<State> Create(object state);
    }
}
