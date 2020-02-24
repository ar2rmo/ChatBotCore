using System;
using Newtonsoft.Json;

namespace BotCore
{
	public interface IUser
	{
		[JsonProperty]
		String Id { get; }
		[JsonProperty]
		String Username { get; }
		[JsonProperty]
		String FirstName { get; }
		[JsonProperty]
		String LastName { get; }
		[JsonProperty]
		String PhoneNumber { get; }
	}
}
