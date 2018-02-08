using System;
using Android.Bluetooth;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
    public class HappimeterService : BluetoothGattService
    {

        //private const string ServiceGuid = "2f234454-cf6d-4a0f-adf2-f4911ba9ffa6"
        private const string ServiceGuid = "F0000000-0000-1000-8000-00805F9B34FB";
        //
        private HappimeterService() : base(uuid: UUID.FromString(ServiceGuid),serviceType: GattServiceType.Primary)
        {
            Characteristics.Add(new HappimeterDataCharacteristic());
            Characteristics.Add(new HappimeterAuthCharacteristic());
            Characteristics.Add(new ReadCharacteristic());

            var descriptorForNotifications = new BluetoothGattDescriptor(UUID.FromString("00002902-0000-1000-8000-00805F9B34FB"), GattDescriptorPermission.Read | GattDescriptorPermission.Write);

            /*
            foreach(var characteristic in Characteristics)  {
                characteristic.AddDescriptor(descriptorForNotifications);
            }*/
        }

        public static HappimeterService Create() {
            return new HappimeterService();
        }
    }
}
