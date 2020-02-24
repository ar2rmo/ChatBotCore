using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BotCore
{
	public interface IChatSession
	{
		Guid ChatId { get; }
	}
}
