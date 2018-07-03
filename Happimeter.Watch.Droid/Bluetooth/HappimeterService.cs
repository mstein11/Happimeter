using System;
using Android.Bluetooth;
using Happimeter.Core.Helper;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
	public class HappimeterService : BluetoothGattService
	{
		private HappimeterService() : base(uuid: UUID.FromString(UuidHelper.AndroidWatchServiceUuidString), serviceType: GattServiceType.Primary)
		{
			AddCharacteristic(new HappimeterDataCharacteristic());
			AddCharacteristic(new HappimeterGenericQuestionCharacteristic());
			AddCharacteristic(new HappimeterMeasurementModeCharacteristic());
			AddCharacteristic(new HappimeterDataNotifyCharacteristic());
			AddCharacteristic(new HappimeterPreSurveyDataCharacteristic());
		}

		public static HappimeterService Create()
		{
			return new HappimeterService();
		}
	}
}
