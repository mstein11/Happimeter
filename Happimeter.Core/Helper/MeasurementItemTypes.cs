using System;
using System.Collections.Generic;

namespace Happimeter.Core.Helper
{
	public static class MeasurementItemTypes
	{

		public const string HeartRate = "Heartrate";
		public const string HeartRateClean = "HeartrateClean";
		public const string Step = "Step";
		public const string Light = "Light";
		public const string AccelerometerX = "AccelerometerX";
		public const string AccelerometerY = "AccelerometerY";
		public const string AccelerometerZ = "AccelerometerZ";
		public const string AccelerometerMagX = "AccelerometerMagX";
		public const string AccelerometerMagY = "AccelerometerMagY";
		public const string AccelerometerMagZ = "AccelerometerMagZ";
		public const string Microphone = "Microphone";
		public const string LocationLat = "LocationLat";
		public const string LocationLon = "LocationLon";
		public const string LocationAlt = "LocationAlt";
		public const string Vmc = "VMC";
		public const string ActivityUnspecific = "ActivityUnspecific";
		public const string ActivityInCar = "ActivityInCar";
		public const string ActivityOnBicycle = "ActivityOnBicycle";
		public const string ActivityOnFoot = "ActivityOnFoot";
		public const string ActivityWalking = "ActivityWalking";
		public const string ActivityRunning = "ActivityRunning";
		public const string ActivityStill = "ActivityStill";
		public const string ProximityRssi = "ProximityRssi";
		public const string ProximityCm = "ProximityCm";

		public static List<string> ActivityTypes => new List<string>{
			ActivityUnspecific,
			ActivityInCar,
			ActivityOnBicycle,
			ActivityOnFoot,
			ActivityWalking,
			ActivityRunning,
			ActivityStill,
			ActivityUnspecific
		};
	}
}
