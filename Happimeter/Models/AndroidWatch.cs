using System;
using System.Diagnostics;
using Plugin.BluetoothLE;

namespace Happimeter.Models
{
    public class AndroidWatch : BluetoothDevice
    {
        public AndroidWatch(IDevice device) : base(device) 
        {

        }

        public static readonly Guid ServiceUuid = Guid.Parse("2f234454-cf6d-4a0f-adf2-f4911ba9ffa6");//maybe instead : 00000009-0000-3512-2118-0009af100700
        public static readonly Guid AuthCharacteristic = Guid.Parse("68b13553-0c4d-43de-8c1c-2b10d77d2d90");
        //public static readonly Guid ButtonTouch = Guid.Parse("1b4dc745-1929-485a-93f6-a76109f02bd6");

        public override IObservable<object> Connect()
        {
            var connection = base.Connect();

            WhenDeviceReady().Subscribe(success =>
            {
                if (success) {
                    try {
                        
                    
                    Device.WhenServiceDiscovered().Subscribe(service => {
                        Debug.WriteLine(service);
                        if (service.Uuid == ServiceUuid)
                        {
                            Debug.WriteLine("Found our HappimeterDataService");
                        }
                    });
                    Device.WhenAnyCharacteristicDiscovered().Subscribe(characteristic =>
                    {
                        Debug.WriteLine(characteristic.Uuid);
                        if (characteristic.Uuid == ServiceUuid)
                        {
                            Debug.WriteLine("Found our AuthCharacteristic");
                        }
                    });
                    } catch (Exception e) {
                        Debug.WriteLine(e.Message);
                    }
                }
            });

            return connection;
        }
    }
}
