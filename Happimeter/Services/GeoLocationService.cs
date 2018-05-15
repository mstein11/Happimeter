using System;
using Plugin.Geolocator;
using System.Threading.Tasks;
using Plugin.Geolocator.Abstractions;

namespace Happimeter.Services
{
	public class GeoLocationService : IGeoLocationService
	{
		public GeoLocationService()
		{
		}

		public bool IsLocationAvailable()
		{
			if (!CrossGeolocator.IsSupported)
				return false;

			return CrossGeolocator.Current.IsGeolocationAvailable;
		}

		public async Task<Position> GetLocation()
		{
			Position position = null;
			try
			{
				var locator = CrossGeolocator.Current;
				locator.DesiredAccuracy = 100;

				position = await locator.GetLastKnownLocationAsync();

				if (position != null)
				{
					//got a cahched position, so let's use it.
					return position;
				}

				if (!IsLocationAvailable() || !locator.IsGeolocationEnabled)
				{
					//not available or enabled
					return null;
				}

				position = await locator.GetPositionAsync(TimeSpan.FromSeconds(20), null, true);

			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to get location: " + ex);
			}

			if (position == null)
				return null;

			var output = string.Format("Time: {0} \nLat: {1} \nLong: {2} \nAltitude: {3} \nAltitude Accuracy: {4} \nAccuracy: {5} \nHeading: {6} \nSpeed: {7}",
					position.Timestamp, position.Latitude, position.Longitude,
					position.Altitude, position.AltitudeAccuracy, position.Accuracy, position.Heading, position.Speed);

			Console.WriteLine(output);

			return position;
		}
	}
}
