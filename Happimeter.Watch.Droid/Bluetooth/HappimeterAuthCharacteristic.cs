using System;
using System.Linq;
using Android.Bluetooth;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
    public class HappimeterAuthCharacteristic : BluetoothGattCharacteristic
    {
        public const string CharacteristicUuid = "68b13553-0c4d-43de-8c1c-2b10d77d2d90";
        private const GattProperty GattProperties = GattProperty.Read | GattProperty.Write;
        private const GattPermission GattPermissions = GattPermission.Read | GattPermission.Write;

        public HappimeterAuthCharacteristic() : base(uuid: UUID.FromString(CharacteristicUuid), properties: GattProperties, permissions: GattPermissions)
        {
            //var descriptor = GetDescriptor(uuid);
            //descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
        }

    }
}
