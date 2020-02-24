using Newtonsoft.Json;

namespace BotCore
{
	public interface ILocation
	{
		[JsonProperty]
		float Latitude { get; }
		[JsonProperty]
		float Longitude { get; }
	}
}
