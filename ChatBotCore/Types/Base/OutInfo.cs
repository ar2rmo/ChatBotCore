using BotCore.Telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotCore.Types.Base
{
    internal class OutInfo
    {
        internal ChatSession Session { get; set; }

        internal IOutgoingMessage OutMsg { get; set; }
    }
}
