using Api.Managers;
using BotCore.Telegram;
using BotCore.Types.Base;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BotCore.Managers
{
    enum ChatState
    {
        Live = 200,
        Cache = 300,
        Wait = 400
    }

    class ChatWatcher
    {
        public Stopwatch Stopwatch;

        public ChatState State;
    }

    class ChatWait
    {
        public IChatSession Session { get; set; }

        public String Value { get; set; }
    }

    public class ChatManager
    {
        [JsonProperty]
        internal Dictionary<Guid, ChatWait> Waiters = new Dictionary<Guid, ChatWait>();
        [JsonIgnore]
        private CacheManager _cache = CacheManager.Manager;
        [JsonIgnore]
        private Dictionary<Guid, IChatProcessor> _chat_procs = new Dictionary<Guid, IChatProcessor>();
        [JsonIgnore]
        private Dictionary<Guid, ChatWatcher> _chat_watch = new Dictionary<Guid, ChatWatcher>();
        [JsonIgnore]
        private Logger Log = new BotLogManager().GetManager<ChatManager>();
        [JsonIgnore]
        private bool _isLive;
        [JsonIgnore]
        private Thread _loop_thread;

        public ChatManager()
        {
            _isLive = true;
            Log.Info("ChatMenager created");
        }

        public IChatProcessor GetChat(Guid guid)
        {
            Log.Debug($"Try find Chat by {guid}");
            if (_chat_procs.ContainsKey(guid))
            {
                Log.Debug("Find in stash");

                return _chat_procs[guid];
            }
            else if(_cache.ContainsKey(guid.ToString()))
            {
                Log.Debug("Find in cache");
                var res = _cache.GetItem<IChatProcessor>(guid.ToString());
         
                return res;
            }
            else if(FileManager.Chat.Contains(guid))
            {
                Log.Debug("Find in serialize");
                var res = FileManager.Chat.GetChat(guid);
                return res;
            }
            else
            {
                return null;
            }
        }

        public void Start()
        {
            Log.Debug("Start catching useless chat");
            _loop_thread = new Thread(Loop);
            _loop_thread.Start();
        }

        public void Stop()
        {
            Log.Debug("Stop catching useless chat");
            _isLive = false;
            _loop_thread.Join(1000);
            _loop_thread = null;
            Save();
        }

        private void Save()
        {
            Log.Debug("Save all chat to file");
            _chat_watch.
                Where(x => x.Value.State == ChatState.Cache).
                Select(x => x.Key).
                ToList().
                ForEach(x => 
                {
                    _chat_procs[x] = _cache.GetItem<IChatProcessor>(x.ToString());
                });

            foreach (var item in _chat_procs)
            {
                FileManager.Chat.SaveChat(item.Key, item.Value);
            }
        }

        public void SaveChat(Guid guid, IChatProcessor chat)
        {
            Log.Debug($"Save chat with {guid}");
            _chat_procs[guid] = chat;

            if (_chat_watch.ContainsKey(guid))
            {
                _chat_watch[guid].State = ChatState.Live;
                _chat_watch[guid].Stopwatch.Restart();
            }
            else
            {
                var watch = new Stopwatch();
                watch.Start();
                _chat_watch[guid] = new ChatWatcher
                {
                    State = ChatState.Live,
                    Stopwatch = watch
                };
            }

            FileManager.Chat.SaveChat(guid,chat);
        }

        public void DeleteChat(Guid guid)
        {
            FileManager.Chat.DeleteChat(guid);
            _chat_watch.Remove(guid);
            _chat_procs.Remove(guid);
            _cache.DeleteItem(guid.ToString());
        }

        private void Loop()
        {
            List<Guid> trash_list;
            while (_isLive)
            {
                trash_list = new List<Guid>();
                foreach (var item in _chat_watch)
                {
                    switch (item.Value.State)
                    {
                        case ChatState.Live:
                            {
                                if (item.Value.Stopwatch.Elapsed.Minutes >= 5)
                                {
                                    Log.Debug($"Save chat {item.Key} to cache");
                                    _cache.SaveItem(item.Key.ToString(), _chat_procs[item.Key]);
                                    _chat_procs.Remove(item.Key);
                                    item.Value.State = ChatState.Cache;
                                    item.Value.Stopwatch.Restart();
                                }
                                break;
                            }
                        case ChatState.Cache:
                            {
                                if (item.Value.Stopwatch.Elapsed.Minutes >= 30)
                                {
                                    Log.Debug($"Save chat {item.Key} to file");
                                    FileManager.Chat.SaveChat(item.Key, _cache.GetItem<IChatProcessor>(item.Key.ToString()));

                                    trash_list.Add(item.Key);
                                }
                                break;
                            }
                    }
                }
                trash_list.ForEach(x => _chat_watch.Remove(x));
                foreach (var item in Waiters)
                {
                    var guid = item.Key;
                    if (int.Parse(item.Value.Value) <= _chat_watch[guid].Stopwatch.Elapsed.Hours)
                    {
                        var msg = FileManager.Massage.GetMsg(guid);
                        var sess = item.Value.Session;

                        OnSendWaitMessage.Invoke(sess, msg).Wait();
                        trash_list.Add(guid);

                        _chat_watch[guid].Stopwatch.Restart();
                    }
                }
                trash_list.ForEach(x => Waiters.Remove(x));
                Thread.Sleep(10000);
            }
        }

        public delegate Task OnMsgWait(IChatSession sess, IOutgoingMessage msg);

        public OnMsgWait OnSendWaitMessage;

        internal void SetWaitMsg(IChatSession session, IOutgoingMessage msg)
        {
            var watch = new Stopwatch();
            watch.Start();
            Guid guid = session.ChatId;
            _chat_watch[guid] = new ChatWatcher
            {
                State = ChatState.Wait,
                Stopwatch = watch
            };
            Waiters[guid] = new ChatWait { Session = session, Value = msg.Text};

            var mToSave = new OutgoingMessage
            {
                Text = "Продовжимо ?)",
                Type = Types.Enums.MsgOutType.Keyboard,
                Keyboard = new Keyboard
                {
                     ColNum = 1,
                     Buttons = new List<Button>
                     {
                         new Button("ТАК") ,
                         new Button("НІ")
                     }
                }
            };

            Log.Debug($"Save MSG with {guid}");

            FileManager.Massage.SaveMsg(guid, mToSave);
        }
    }
}