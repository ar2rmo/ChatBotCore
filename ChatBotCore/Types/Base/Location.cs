namespace BotCore
{
	public class Location : ILocation
	{
		public Location()
		{

		}

		public Location(float latitude, float longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}
		public float Latitude { get; set; }

		public float Longitude { get; set; }

		public override string ToString()
		{
			return $"Location : {{Latitude :\"{Latitude}\", Longitude :\"{Longitude}\"}}";
		}
	}
}
