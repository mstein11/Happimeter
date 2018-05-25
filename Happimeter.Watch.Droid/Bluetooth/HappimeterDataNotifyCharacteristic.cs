using System;
using Android.Bluetooth;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Happimeter.Watch.Droid.Workers;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
	public class HappimeterDataNotifyCharacteristic : BluetoothGattCharacteristic
	{
		private const GattProperty GattProperties = GattProperty.Notify;
		private const GattPermission GattPermissions = GattPermission.Read | GattPermission.Write;
		public HappimeterDataNotifyCharacteristic() : base(uuid: UUID.FromString(UuidHelper.DataExchangeNotifyCharacteristicUuidString), properties: GattProperties, permissions: GattPermissions)
		{
			var uuid = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
			//this.WriteType = GattWriteType.Default;
			var configDescriptor = new BluetoothGattDescriptor(uuid, GattDescriptorPermission.Read | GattDescriptorPermission.Write);
			AddDescriptor(configDescriptor);
		}
	}
}
