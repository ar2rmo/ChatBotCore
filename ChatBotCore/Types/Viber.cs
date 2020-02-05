using Api.Managers;
using BotCore.Managers;
using BotCore.Telegram;
using BotCore.Types.Enums;
using Newtonsoft.Json;
using NLog;
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
    }

    internal class IncomingMessage : IIncomingMessage
    {
        Logger Log;

        public IncomingMessage()
        {
            Text = "init";
            Type = MessageInType.Text;
        }

        public IncomingMessage(CallbackData data)
        {
            Text = "bullshit";
            Log = new BotLogManager().GetManager<IncomingMessage>();
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

    public class AciveViberBot : BotBase, IBot
    {
        [JsonIgnore]
        protected ViberBotClient _cli;
        [JsonIgnore]
        protected bool isListening;
        [JsonIgnore]
        protected HttpListener listener = new HttpListener();
        [JsonIgnore]
        protected Thread _thread;
        [JsonIgnore]
        TaskManager taskManager;

        public AciveViberBot()
        {
            
        }

        public AciveViberBot(IConfiguration configuration) : base(configuration)
        {
            Log = new BotLogManager().GetManager<TelegramBotBase>();
            _cli = new ViberBotClient(configuration.Token);
        }

        public override void Start()
        {
            Log.Debug("Init listening");
            _cli = new ViberBotClient(_conf.Token);
            Log.Debug($"{this.GetType()} start listening telegram msgs");
            _chat_manager.Start();
            _thread = new Thread(Loop);
            _thread.Start();
            Task t = _cli.SetWebhookAsync(_conf.WebHook);
            Task.WaitAll(t);
            _chat_manager.OnSendWaitMessage = async (IChatSession sess, IOutgoingMessage msg) => await SendMessageAsync(sess, msg);
        }

        public override void Stop()
        {
            isListening = false;
            _thread.Join(1000);
            _thread = null;
            _chat_manager.Stop();
            Log.Debug($"{this.GetType()} stop listening telegram msgs");
        }

        public void Loop()
        {
            listener.Prefixes.Add(_conf.UriListener);
            listener.Start();
            isListening = true;


            taskManager = new TaskManager();
            while (isListening)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerResponse response = context.Response;
                HttpListenerRequest request = context.Request;

                response.StatusCode = (int)HttpStatusCode.OK;
                using (Stream stream = response.OutputStream)
                {
                    Log.Debug("OK response send!");
                }
                while (taskManager.IsAbuse)
                {
                    Thread.Sleep(100);
                };
                Task task = CentralRoadAsync(request);

                taskManager.Add(task);
            }
            
            listener.Stop();
            listener.Close();

            taskManager.WaitAll();
        }

        private async Task CentralRoadAsync(HttpListenerRequest request)
        {

            CallbackData data = await BotJsonManager.ParseFromRequest<CallbackData>(request);
            switch (data.Event)
            {
                case EventType.Delivered:
                    break;
                case EventType.Seen:
                    {
                        Log.Info($"Get seen resp");
                    }
                    break;
                case EventType.Failed:
                    {
                        break;
                    }
                case EventType.Subscribed:
                    {
                        var m = new IncomingMessage();
                        var s = new ChatSession(data.User.Id);
                        await ProcessIncomingMessageAsync(m, s);
                        break;
                    }
                case EventType.Unsubscribed:
                    {
                        var s = new ChatSession(data.UserId);
                        _chat_manager.DeleteChat(s.ChatId);
                        break;
                    }
                case EventType.ConversationStarted:
                    {
                        var s = new ChatSession(data.User.Id);
                        var outMsg = new TextMessage
                        {
                            Text = "Dlia podalshogyi vzaemodiyi vidparte msg",
                            Receiver = s.InternalChatId 
                        };
                        await _cli.SendTextMessageAsync(outMsg);
                        break;
                    }
                case EventType.Message:
                    {
                        var m = new IncomingMessage(data);
                        var s = new ChatSession(data.Sender.Id);
                        await ProcessIncomingMessageAsync(m,s);
                        break;
                    }
                case EventType.Webhook:
                    {
                        break;
                    }
                case EventType.Action:
                    break;
                default:
                    break;
            }
        }

        public override async Task SendMessageAsync(IChatSession sess, IOutgoingMessage msg)
        {
            Log.Debug($"SendMessageAsync with sess {sess.ChatId} and out msg {msg}");
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
                        _chat_manager.SetWaitMsg(sess, msg);
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