using System;
using Android.Bluetooth;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
    public class HappimeterService : BluetoothGattService
    {

        private const string ServiceGuid = "2f234454-cf6d-4a0f-adf2-f4911ba9ffa6";

        private HappimeterService() : base(uuid: UUID.FromString(ServiceGuid),serviceType: GattServiceType.Primary)
        {
            Characteristics.Add(new HappimeterDataCharacteristic());
            Characteristics.Add(new HappimeterAuthCharacteristic());
        }

        public static HappimeterService Create() {
            return new HappimeterService();
        }
    }
}
