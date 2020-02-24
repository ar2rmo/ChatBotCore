using BotCore.Types.Enums;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Viber.Bot;

namespace BotCore.Viber
{
	internal class ChatSession : IChatSession
	{
		private String rawId;

		public String InternalChatId {
			get
			{
				return rawId;
			}
			set
			{
				rawId = value;
				ChatId = new Guid(Convert.FromBase64String(rawId));
			}
		}

		public Guid ChatId { get; private set; }

		public ChatSession(String id)
		{
			InternalChatId = id;
		}

		public ChatSession(IChatSession source)
		{
			var ba = source.ChatId.ToByteArray();
			InternalChatId = Convert.ToBase64String(ba);
		}
	}

	internal class IncomingMessage : IIncomingMessage
	{
		public IncomingMessage()
		{
			Text = "init";
			Type = MessageInType.Text;
		}

		public IncomingMessage(CallbackData data)
		{
			Text = "bullshit";
			if(data.Event == EventType.Message)
			switch (data.Message.Type)
			{
				case MessageType.Text:
					{
						var messsage = data.Message as TextMessage;
						Text = messsage.Text;
						Type = MessageInType.Text;
					}
					break;
				case MessageType.Picture:
					break;
				case MessageType.Video:
					break;
				case MessageType.File:
					break;
				case MessageType.Location:
					{
						var message = data.Message as LocationMessage;
						Location = new Location
						{
							Latitude = (float)message.Location.Lat,
							Longitude = (float)message.Location.Lon
						};
						Type = MessageInType.Location;
					}
					break;
				case MessageType.Contact:
					{
						var message = data.Message as ContactMessage;
						User = new User
						{
							PhoneNumber = message.Contact.TN,
							FirstName = message.Contact.Name,
							Id = data.Sender.Id
						};
						Type = MessageInType.Contact;
					}
					break;
				case MessageType.Sticker:
					break;
				case MessageType.CarouselContent:
					break;
				case MessageType.Url:
					break;
				default:
					break;
			}
		}

		public String Text { get; private set; }

		public MessageInType Type { get; set; }

		public User User{ get; set; }

		public Location Location { get; set; }

		public override string ToString()
		{
			return $"IncomingMessage :{{Type :\"{Type}\"," +
				$" \"{Location}\"," +
				$" \"{User}\"," +
				$" Text :\"{Text}\"}}";
		}
	}

	public class ViberBot : BotBase, IBot
	{
		[JsonIgnore]
		protected ViberBotClient _cli;
		
		public ViberBot(IConfiguration configuration) : base(configuration)
		{
			_cli = new ViberBotClient(configuration.Token);
		}

		public override async Task InitAsync()
		{
			await _cli.SetWebhookAsync(conf.WebHook);
		}

		protected override bool ParseIncomingMessage(String json, out IChatSession s, out IIncomingMessage m)
		{
			s = null;
			m = null;
			CallbackData data = JsonConvert.DeserializeObject<CallbackData>(json);
			switch (data.Event)
			{
				case EventType.Delivered:
					return false;
				case EventType.Seen:
					return false;
				case EventType.Failed:
					return false;
				case EventType.Subscribed:
					{
						m = new IncomingMessage();
						s = new ChatSession(data.User.Id);
						return true;
					}
				case EventType.Unsubscribed:
					{
						s = new ChatSession(data.UserId);
						return false;
					}
				case EventType.ConversationStarted:
					{
						return false;
					}
				case EventType.Message:
					{
						m = new IncomingMessage(data);
						s = new ChatSession(data.Sender.Id);
						return true;
					}
				case EventType.Webhook:
					{
						return false;
					}
				case EventType.Action:
					return false;
				default:
					return false;
			}
		}

		public override async Task SendMessageAsync(IChatSession sess, IOutgoingMessage msg)
		{
			if (!(sess is ChatSession)) throw new Exception("Bad chat session type");
			var s = sess as ChatSession;
			switch (msg.Type)
			{
				case MsgOutType.Text:
					{
						var m = new TextMessage
						{
							Text = msg.Text,
							Receiver = s.InternalChatId
						};
						await _cli.SendTextMessageAsync(m);
						break;
					}
				case MsgOutType.Keyboard:
					{
						var keyboardMsg = new KeyboardMessage
						{
							Text = msg.Text,
							Receiver = s.InternalChatId,
							Keyboard = GetViberKeyboard(1,msg.Keyboard.Buttons.ToArray()),
							MinApiVersion = 3
						};
						await _cli.SendKeyboardMessageAsync(keyboardMsg);
						break;
					}
				case MsgOutType.Photo:
					break;
				case MsgOutType.Wait:
					{
						var m = new TextMessage
						{
							Text = $"Нагадування встановлено",
							Receiver = s.InternalChatId
						};
						await _cli.SendTextMessageAsync(m);
						break;
					}
				default:
					break;
			}
		}

		private global::Viber.Bot.Keyboard GetViberKeyboard(int col, IButton[] buttons)
		{
			var kbd = new global::Viber.Bot.Keyboard();

			var keyBoard = KeyBoardBuilder.CreateCol<KeyboardButton>(buttons.Count());

			for (int i = 0; i < keyBoard.Count(); i++)
			{
				keyBoard[i].Text = buttons[i].Text;
				keyBoard[i].TextSize = TextSize.Regular;
				keyBoard[i].BackgroundColor = "#00a3cc";
				keyBoard[i].ActionBody = buttons[i].Text;
				keyBoard[i].ActionType = KeyboardActionType.Reply;

				if (buttons[i].Key == "RequestContact")
					keyBoard[i].ActionType = KeyboardActionType.SharePhone;
				else if (buttons[i].Key == "RequestLocation")
					keyBoard[i].ActionType = KeyboardActionType.LocationPicker;
			}
			kbd.Buttons = keyBoard;
			kbd.BackgroundColor = "#80e5ff";
			kbd.DefaultHeight = false;
			return kbd;
		}

		public override string ToString()
		{
			return $"ActiveViberBot :{{BotClient :\"{_cli}\", \"{base.ToString()}\"}}";
		}
	}
}