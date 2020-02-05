using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Api.Managers;
using BotCore.Managers;
using BotCore.Types.Enums;
using BotCore.Types.Base;

namespace BotCore.Telegram
{

    internal class ChatSession : IChatSession
    {
        [JsonProperty]
        public Int64 InternalChatId { get; protected set; }
        [JsonIgnore]
        public Guid ChatId
        {
            get
            {
                var ba = BitConverter.GetBytes(InternalChatId);
                Byte[] arr = new Byte[16];
                Array.Copy(ba, arr, 8);
                Array.Copy(ba, 0, arr, 8, 8);
                return new Guid(arr);
            }
        }

        public ChatSession(Int64 id)
        {
            InternalChatId = id;
        }
    }

    internal class IncomingMessage : IIncomingMessage
    {
        Logger Log;
        public IncomingMessage(Message message)
        {
            Text = "bullshit";
            Log = new BotLogManager().GetManager<IncomingMessage>();
            try
            {
                Type = (MessageInType)Enum.Parse(typeof(MessageInType), $"{message.Type}");
            }
            catch (Exception)
            {
                Log.Warn("There is no such of Message type");
                Type = MessageInType.Uknown;
            }

            switch (Type)
            {
                case MessageInType.Uknown:
                    break;
                case MessageInType.Text:
                    Text = message.Text;
                    break;
                case MessageInType.Contact:
                    User = new User
                    {
                        Id = message.Contact.UserId.ToString(),
                        FirstName = message.Contact.FirstName,
                        LastName = message.Contact.LastName,
                        PhoneNumber = message.Contact.PhoneNumber
                    };
                    break;
                case MessageInType.Location:
                    Location = new Location
                    {
                        Latitude = message.Location.Latitude,
                        Longitude = message.Location.Longitude
                    };
                    break;
                case MessageInType.Photo:
                    break;
                default:
                    break;
            }
            
        }

        public String Text { get; private set; }

        public MessageInType Type { get; set; }

        public User User { get; set; }

        public Location Location { get; set; }

        public override string ToString()
        {
            return $"IncomingMessage :{{Type :\"{Type}\"," +
                $" \"{Location}\"," +
                $" \"{User}\"," +
                $" Text :\"{Text}\"}}";
        }

    }

    public class TelegramBotBase : BotBase
    {
        [JsonIgnore]
        protected TelegramBotClient _cli;
        [JsonProperty]
        private Queue<OutInfo> OutQueue = new Queue<OutInfo>();

        public TelegramBotBase(IConfiguration configuration) : base(configuration)
        {
            Log = new BotLogManager().GetManager<TelegramBotBase>();
            Log.Debug($"Construct TelegramBotBase from conf: {configuration}");
        }

        public override async Task SendMessageAsync(IChatSession sess, IOutgoingMessage msg)
        {
            OutQueue.Enqueue(new OutInfo { Session = sess as ChatSession, OutMsg = msg });
            while (OutQueue.Count > 0) 
            {
                var letter = OutQueue.Dequeue();

                Log.Debug($"SendMessageAsync with sess {letter.Session.ChatId} and out msg {letter.OutMsg}");
                if (!(sess is ChatSession)) throw new Exception("Bad chat session type");
                var s = sess as ChatSession;
                switch (letter.OutMsg.Type)
                {
                    case MsgOutType.Text:
                        {
                            var m =  await _cli.SendTextMessageAsync(letter.Session.InternalChatId, letter.OutMsg.Text);
                        }
                        break;
                    case MsgOutType.Keyboard:
                        {
                            var m = await _cli.SendTextMessageAsync
                                (
                                    letter.Session.InternalChatId,
                                    letter.OutMsg.Text,
                                    ParseMode.Markdown,
                                    false, false, 0,
                                    GetTelegramKeyboard(letter.OutMsg.Keyboard)
                                );
                        }
                        break;
                    case MsgOutType.Photo:
                        break;
                    case MsgOutType.Wait:
                        {
                            _chat_manager.SetWaitMsg(letter.Session, letter.OutMsg);
                            var m = await _cli.SendTextMessageAsync(letter.Session.InternalChatId, $"Нагадування встановлено");
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        protected IReplyMarkup GetTelegramKeyboard(IKeyboard keyboard)
        {
            if (keyboard.Key == "InLine")
            {
                return GetTelegramKeyboardInLine(keyboard.ColNum, keyboard.Buttons.ToArray());
            }
            else
            {
                return GetTelegramKeyboardReply(keyboard.ColNum, keyboard.Buttons.ToArray());
            }
        }

        protected ReplyKeyboardMarkup GetTelegramKeyboardReply(int col, IButton[] buttons)
        {
            var keyBoard = KeyBoardBuilder.CreateKeyBoard<KeyboardButton>(col, buttons.Count());
            int counter = 0;

            for (int i = 0; i < keyBoard.Count(); i++)
            {
                for (int j = 0; j < keyBoard[i].Count(); j++)
                {
                    keyBoard[i][j].Text = buttons[counter].Text;
                    if (buttons[counter].Key == "RequestContact")
                    {
                        keyBoard[i][j].RequestContact = true;
                    }
                    else if (buttons[counter].Key == "RequestLocation")
                    {
                        keyBoard[i][j].RequestLocation = true;
                    }
                    counter++;
                }
            }
            var rkm = new ReplyKeyboardMarkup(keyBoard, true, true);

            return rkm;
        }

        protected InlineKeyboardMarkup GetTelegramKeyboardInLine(int col, IButton[] buttons)
        {
            var keyBoard = KeyBoardBuilder.CreateKeyBoard<InlineKeyboardButton>(col, buttons.Count());
            int counter = 0;

            for (int i = 0; i < keyBoard.Count(); i++)
            {
                for (int j = 0; j < keyBoard[i].Count(); j++)
                {
                    keyBoard[i][j].Text = buttons[counter].Text;
                    keyBoard[i][j].CallbackData = buttons[counter].Key;
                    counter++;
                }
            }
            var rkm = new InlineKeyboardMarkup(keyBoard);

            return rkm;
        }

        public override string ToString()
        {
            return $"TelegramBotBase :{{BotClient :\"{_cli}\", \"{base.ToString()}\"}}";
        }

        public override void Start()
        {
            _cli = new TelegramBotClient(_conf.Token);
        }

        public override void Stop()
        {

        }
    }

    public class PassiveBot : TelegramBotBase, IBot
    {
        public PassiveBot(IConfiguration configuration) : base(configuration)
        {
            
        }

        private void OnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            var m = new IncomingMessage(message);
            var s = new ChatSession(message.Chat.Id);

            ProcessIncomingMessageAsync(m, s).Wait();
        }

        public void OnCallbackQueryRecievd(object sender, CallbackQueryEventArgs callback)
        {
            var message = callback.CallbackQuery.Message;
            message.Text = callback.CallbackQuery.Data;
            var m = new IncomingMessage(message);
            var s = new ChatSession(message.Chat.Id);

            ProcessIncomingMessageAsync(m, s).Wait();
        }

        public override void Start()
        {
            _cli = new TelegramBotClient(_conf.Token);
            _cli.OnMessage += OnMessageReceived;
            _cli.OnCallbackQuery += OnCallbackQueryRecievd;
            _cli.SetWebhookAsync("");
            _cli.StartReceiving();
            _chat_manager.OnSendWaitMessage = async (IChatSession sess, IOutgoingMessage msg) => await SendMessageAsync(sess, msg);
        }

        public override void Stop()
        {
            _cli.StopReceiving();
        }
    }

    public class ActiveTelegramBot : TelegramBotBase, IBot
    {
        [JsonIgnore]
        protected bool isListening;
        [JsonIgnore]
        Thread _thread;
        [JsonIgnore]
        protected HttpListener listener = new HttpListener();
        [JsonIgnore]
        TaskManager taskManager;

        public ActiveTelegramBot(IConfiguration configuration) : base(configuration)
        {

        }

        private void Loop()
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
                    Log.Debug("Task manager is Abuse.Wait tasks!");
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
            Update update = await BotJsonManager.ParseFromRequest<Update>(request);
            switch (update.Type)
            {
                case UpdateType.Unknown:
                    break;
                case UpdateType.Message:
                    {
                        var m = new IncomingMessage(update.Message);
                        var s = new ChatSession(update.Message.Chat.Id);

                        await ProcessIncomingMessageAsync(m, s);
                        break;
                    }
                case UpdateType.InlineQuery:
                    break;
                case UpdateType.ChosenInlineResult:
                    break;
                case UpdateType.CallbackQuery:
                    {
                        var message = update.CallbackQuery.Message;
                        message.Text = update.CallbackQuery.Data;
                        var m = new IncomingMessage(message);
                        var s = new ChatSession(message.Chat.Id);

                        await ProcessIncomingMessageAsync(m, s);
                        break;
                    }
                case UpdateType.EditedMessage:
                    break;
                case UpdateType.ChannelPost:
                    break;
                case UpdateType.EditedChannelPost:
                    break;
                case UpdateType.ShippingQuery:
                    break;
                case UpdateType.PreCheckoutQuery:
                    break;
                default:
                    break;
            }
        }

        public override void Start()
        {
            _cli = new TelegramBotClient(_conf.Token);
            _chat_manager.Start();
            Log.Debug("Init listening");

            Log.Debug($"{this.GetType()} start listening telegram msgs");
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

        public override string ToString()
        {
            return $"ActiveTelegramBot :{{Listener :\"{listener}\"," +
                $"Uri \"{_conf.UriListener}\"," +
                $"isListening :\"{isListening}\"," +
                $" \"{base.ToString()}\"}}";
        }
    }
}