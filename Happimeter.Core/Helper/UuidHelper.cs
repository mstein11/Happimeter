using System;
using System.Collections.Generic;
namespace Happimeter.Core.Helper
{
	public static class UuidHelper
	{
		public const string ProximityBeaconUuidString = "f786ee9f-5656-49ab-980f-234fe67f412f";
		public static Guid ProximityBeaconUuid => Guid.Parse(ProximityBeaconUuidString);

		public const string BeaconUuidString = "6c2e3172-768b-4d3e-a47b-01470fe908a4";
		public static Guid BeaconUuid => Guid.Parse(BeaconUuidString);

		public const string WakeupBeaconUuidString = "31c668d3-6b81-4be2-876c-228ed4f88df8";
		public static Guid WakeupBeaconUuid => Guid.Parse(WakeupBeaconUuidString);

		public const string AndroidWatchServiceUuidString = "0000F0F0-0000-1000-8000-00805F9B34FB";
		public static Guid AndroidWatchServiceUuid => Guid.Parse(AndroidWatchServiceUuidString);

		public const string AndroidWatchAuthServiceUuidString = "0000F0F1-0000-1000-8000-00805F9B34FB";
		public static Guid AndroidWatchAuthServiceUuid => Guid.Parse(AndroidWatchServiceUuidString);

		public const string DataExchangeCharacteristicUuidString = "7918ec07-2ba4-4542-aa13-0a10ff3826ba";
		public static Guid DataExchangeCharacteristicUuid => Guid.Parse(DataExchangeCharacteristicUuidString);

		public const string DataExchangeNotifyCharacteristicUuidString = "964699c5-c9ff-486c-a833-d109b5700433";
		public static Guid DataExchangeNotifyCharacteristicUuid => Guid.Parse(DataExchangeNotifyCharacteristicUuidString);

		public const string AuthCharacteristicUuidString = "68b13553-0c4d-43de-8c1c-2b10d77d2d90";
		public static Guid AuthCharacteristicUuid => Guid.Parse(AuthCharacteristicUuidString);

		public const string GenericQuestionCharacteristicUuidString = "679a39d2-1d7c-11e8-b467-0ed5f89f718b";
		public static Guid GenericQuestionCharacteristicUuid => Guid.Parse(GenericQuestionCharacteristicUuidString);

		public const string MeasurementModeCharacteristicUuidString = "347b6858-dfb1-49bf-a615-3320ecaf6920";
		public static Guid MeasurementModeCharacteristicUuid => Guid.Parse(MeasurementModeCharacteristicUuidString);

		public const byte BeaconManufacturerId = 0x004C;
		public const int TxPowerLevel = -56;
		//Layout for iBeacon
		public const string BeaconLayout = "m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24";

		public static List<Guid> KnownCharacteristics()
		{
			return new List<Guid>() {
				DataExchangeCharacteristicUuid,
				AuthCharacteristicUuid,
				GenericQuestionCharacteristicUuid,
				MeasurementModeCharacteristicUuid,
				DataExchangeNotifyCharacteristicUuid
			};
		}
	}
}
