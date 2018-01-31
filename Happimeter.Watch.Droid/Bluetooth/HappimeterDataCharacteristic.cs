using System;
using System.Linq;
using Android.Bluetooth;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
    public class HappimeterDataCharacteristic : BluetoothGattCharacteristic
    {
        public const string CharacteristicUuid = "7918ec07-2ba4-4542-aa13-0a10ff3826ba";
        private const GattProperty GattProperties = GattProperty.Read | GattProperty.Notify;
        private const GattPermission GattPermissions = GattPermission.Read;

        public HappimeterDataCharacteristic() : base(uuid: UUID.FromString(CharacteristicUuid), properties: GattProperties, permissions: GattPermissions)
        {
            var uuid = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
            this.WriteType = GattWriteType.Default;
            //var descriptor = GetDescriptor(uuid);
            //descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
        }

    }
}
