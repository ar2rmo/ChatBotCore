using Newtonsoft.Json;
using System;

namespace BotCore.Types.Base
{
	public class Configure : IConfiguration
	{
		public String Token { get; set; }
		public String WebHook { get; set; }
	}
}
