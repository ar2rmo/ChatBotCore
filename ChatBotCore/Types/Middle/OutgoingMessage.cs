using BotCore.Types.Enums;
using System;

namespace BotCore
{
	public class OutgoingMessage : IOutgoingMessage
	{
		public OutgoingMessage()
		{
		}

		public OutgoingMessage(String text)
		{
			Text = text;
		}

		public OutgoingMessage(String text, IKeyboard keyboard) : this(text)
		{
			Keyboard = keyboard;
		}

		public String Text { get; set; }
		public IKeyboard Keyboard { get; set; }
		public MsgOutType Type { get; set; }

		public override string ToString()
		{
			return $"OutgoingMessage :{{Text :\"{Text}\", Type :\"{Type}\", :\"{Keyboard}\", }}";
		}
	}

}
