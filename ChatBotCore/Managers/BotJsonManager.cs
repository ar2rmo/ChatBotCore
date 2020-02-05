using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace BotCore.Managers
{
    public class BotJsonManager
    {
        public static async Task<T> ParseFromRequest<T>(HttpListenerRequest request) where T : class
        {
            var size = request.ContentLength64;
            byte[] buff = new byte[size];

            using (Stream stream = request.InputStream)
            {
                await stream.ReadAsync(buff, 0, buff.Length);
            }

            string tmps = Encoding.UTF8.GetString(buff);

            T res  = JsonConvert.DeserializeObject<T>(tmps);

            return res;
        }
    }
}