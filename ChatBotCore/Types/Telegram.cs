using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
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

        public ChatId GetChatId()
        {
            return new ChatId(InternalChatId);
        }

        public ChatSession(IChatSession source)
        {
            var ba = source.ChatId.ToByteArray();
            var i1 = BitConverter.ToInt64(ba, 0);
            var i2 = BitConverter.ToInt64(ba, 8);
            if (i1 != i2) throw new Exception("Bad ChatId format");
            InternalChatId = i1;
        }
    }

    internal class IncomingMessage : IIncomingMessage
    {
        public IncomingMessage(Message message)
        {
            Text = "bullshit";
            try
            {
                Type = (MessageInType)Enum.Parse(typeof(MessageInType), $"{message.Type}");
            }
            catch (Exception)
            {
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

    public class TelegramBot : BotBase, IBot
    {
        protected TelegramBotClient _cli;

        public TelegramBot(IConfiguration configuration) : base(configuration)
        {
            
        }

        public override async Task SendMessageAsync(IChatSession sess, IOutgoingMessage msg)
        {
            var ss = new ChatSession(sess);
            switch (msg.Type)
            {
                case MsgOutType.Text:
                    {
                        await _cli.SendTextMessageAsync(ss.GetChatId(), msg.Text);
                    }
                    break;
                case MsgOutType.Keyboard:
                    {
                        var m = await _cli.SendTextMessageAsync
                            (
                                ss.GetChatId(),
                                msg.Text,
                                ParseMode.Markdown,
                                false, false, 0,
                                GetTelegramKeyboard(msg.Keyboard)
                            );
                    }
                    break;
                case MsgOutType.Photo:
                    break;
                case MsgOutType.Wait:
                    {
                        var m = await _cli.SendTextMessageAsync(ss.GetChatId(), $"Нагадування встановлено");
                        break;
                    }
                default:
                    break;
            }
        }

        protected override bool ParseIncomingMessage(String json, out IChatSession s, out IIncomingMessage m)
        {
            s = null;
            m = null;
            Update update = JsonConvert.DeserializeObject<Update>(json);
            switch (update.Type)
            {
                case UpdateType.Unknown:
                    return false;
                case UpdateType.Message:
                    {
                        m = new IncomingMessage(update.Message);
                        s = new ChatSession(update.Message.Chat.Id);
                        return true;
                    }
                case UpdateType.InlineQuery:
                    return false;
                case UpdateType.ChosenInlineResult:
                    return false;
                case UpdateType.CallbackQuery:
                    {
                        var message = update.CallbackQuery.Message;
                        message.Text = update.CallbackQuery.Data;
                        m = new IncomingMessage(message);
                        s = new ChatSession(message.Chat.Id);
                        return true;
                    }
                case UpdateType.EditedMessage:
                    return false;
                case UpdateType.ChannelPost:
                    return false;
                case UpdateType.EditedChannelPost:
                    return false;
                case UpdateType.ShippingQuery:
                    return false;
                case UpdateType.PreCheckoutQuery:
                    return false;
                default:
                    return false;
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
    }
}