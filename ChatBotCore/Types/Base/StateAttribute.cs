using System;

namespace BotCore
{
    [AttributeUsage(AttributeTargets.Class)]
    public class StateAttribute : Attribute
    {
        public String State { get; }

        public StateAttribute()
        {

        }

        public StateAttribute(object state)
        {
            State = state.ToString();
        }
    }
}
