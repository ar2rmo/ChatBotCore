namespace BotCore.Types
{
    interface IReclaimble<State>
    {
        State NextState { get; }

        State CurentState { get; }
    }
}
