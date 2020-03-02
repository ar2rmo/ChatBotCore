using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace BotCore
{
	public class Keyboard : IKeyboard
	{
		private readonly bool _isRootContinue;

		private IEnumerable<IButton> _buttons;

		public Keyboard()
		{
			_isRootContinue = false;
		}

		public Keyboard(bool isRootContinue)
		{
			ColNum = 1;
			_isRootContinue = isRootContinue;
		}

		public Keyboard(int colNum, IEnumerable<IButton> buttons, bool isRootContinue) : this(isRootContinue)
		{
			ColNum = colNum;
			Buttons = buttons;
		}

		public Keyboard(int colNum, IEnumerable<IButton> buttons)
		{
			ColNum = colNum;
			Buttons = buttons;
		}

		[JsonProperty]
		public IEnumerable<IButton> Buttons
		{
			get => _isRootContinue && _buttons == null ? new List<Button> { new Button("До меню")} : _buttons;
			set
			{
				var btns = value;
				if(_isRootContinue )(btns as List<Button>).Add(new Button("До меню"));

				_buttons = btns;
			} 
		}
		[JsonProperty]
		public int ColNum { get; set; }
		[JsonProperty]
		public string Key { get; set; }

		public override string ToString()
		{
			var buttLog = "{{";
			Buttons.ToList().ForEach(x => buttLog += $"\"{x}\", ");
			buttLog = buttLog.Remove(buttLog.Length - 2, 2) + "}}";
			return $"Keyboard :{{Key :\"{Key}\", ColNum :\"{ColNum}\", Buttons :\"{buttLog}\"}}";
		}
	}
}
