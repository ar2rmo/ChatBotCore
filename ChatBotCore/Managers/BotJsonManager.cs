using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace BotCore.Managers
{
    public class BotJsonManager
    {
        static Logger Log = new BotLogManager().GetManager<BotJsonManager>();

        public static async Task<T> ParseFromRequest<T>(HttpListenerRequest request) where T : class
        {
            var size = request.ContentLength64;
            byte[] buff = new byte[size];

            using (Stream stream = request.InputStream)
            {
                Log.Debug("Geting request!");
                await stream.ReadAsync(buff, 0, buff.Length);
            }

            string tmps = Encoding.UTF8.GetString(buff);

            T res  = JsonConvert.DeserializeObject<T>(tmps);
            Log.Debug($"Parse to {res} from {tmps}");

            return res;
        }
    }
}