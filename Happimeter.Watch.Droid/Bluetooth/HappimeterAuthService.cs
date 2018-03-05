using System;
using Android.Bluetooth;
using Happimeter.Core.Helper;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
    public class HappimeterAuthService : BluetoothGattService
    {
        public HappimeterAuthService() : base(uuid: UUID.FromString(UuidHelper.AndroidWatchAuthServiceUuidString), serviceType: GattServiceType.Primary)
        {
            var authCharacteristic = new HappimeterAuthCharacteristic();
            var descriptorForNotifications = new BluetoothGattDescriptor(UUID.FromString("00002902-0000-1000-8000-00805F9B34FB"), GattDescriptorPermission.Read | GattDescriptorPermission.Write);
            authCharacteristic.AddDescriptor(descriptorForNotifications);
            Characteristics.Add(authCharacteristic);
        }

        public static HappimeterAuthService Create()
        {
            return new HappimeterAuthService();
        }
    }
}
