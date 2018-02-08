using System;
using Android.Bluetooth;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
    public class ReadCharacteristic : BluetoothGattCharacteristic
    {

        public const string CharacteristicUuid = "7918ec07-2ba4-4542-aa13-0a10ff3826bb";
        private const GattProperty GattProperties = GattProperty.Read;
        private const GattPermission GattPermissions = GattPermission.ReadEncryptedMitm;
                                                                     

       
        public ReadCharacteristic() : base(UUID.FromString(CharacteristicUuid), GattProperties, GattPermissions)
        {
        }


    }
}
