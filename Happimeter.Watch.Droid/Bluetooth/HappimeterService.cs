using System;
using Android.Bluetooth;
using Happimeter.Core.Helper;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
    public class HappimeterService : BluetoothGattService
    {

        //private const string ServiceGuid = "2f234454-cf6d-4a0f-adf2-f4911ba9ffa6"
        private const string ServiceGuid = "F0000000-0000-1000-8000-00805F9B34FB";
        //
        private HappimeterService() : base(uuid: UUID.FromString(UuidHelper.AndroidWatchServiceUuidString),serviceType: GattServiceType.Primary)
        {
            AddCharacteristic(new HappimeterDataCharacteristic());
            AddCharacteristic(new HappimeterGenericQuestionCharacteristic());
        }

        public static HappimeterService Create() {
            return new HappimeterService();
        }
    }
}
