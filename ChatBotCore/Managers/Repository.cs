using Api.Managers;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BotCore.Managers
{
    public class Repository
    {
        private static Logger Log = new BotLogManager().GetManager<Repository>();

        public static void SaveDictionary<T>(Dictionary<Guid,T> dictionary)
        {
            dictionary.ToList().ForEach(x => Log.Debug($"Save {x.Value} with key {x.Key} to db"));
        }

        public IEnumerable<T> GetItemList<T,K>(K key)
        {
            var res = new List<T>();
            return res;
        }

        public void SaveItem<T>(T item)
        {
            Log.Debug($"Save item {item}");
        }

        public void SaveItems<T>(IEnumerable<T> items)
        { 
            items.ToList().ForEach(x => SaveItem(x));
        }

        public T GetItem<T>(string key) where T : new()
        {
            var res = new T();
            return res;
        }

        public T GetItem<T>(Guid guid) where T : new()
        {
            var res = new T();
            return res;
        }

        public T GetItem<T>(long id) where T : new()
        {
            var res = new T();
            return res;
        }

        public void DeleteItem(long id)
        {
            if (true)
            {
            }
        }

    }
}
