using System;
using System.Diagnostics;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Interfaces;
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
        public static readonly Guid DataCharacterisic = Guid.Parse("7918ec07-2ba4-4542-aa13-0a10ff3826ba");

        public override IObservable<object> Connect()
        {
            var connection = base.Connect();

            WhenDeviceReady().Subscribe(success =>
            {
                if (success) {
                    try {
                        Device.WhenServiceDiscovered().Subscribe(service =>
                        {
                            Debug.WriteLine(service);
                            if (service.Uuid == ServiceUuid)
                            {
                                Debug.WriteLine("Found our HappimeterDataService");
                            }
                        });
                        Device.WhenAnyCharacteristicDiscovered().Subscribe(characteristic =>
                        {
                            Debug.WriteLine($"Found characteristic: {characteristic.Uuid}");
  
                            if (characteristic.Uuid == AuthCharacteristic)
                            {
                                
                                var message = System.Text.Encoding.UTF8.GetBytes("Hallo");
                                characteristic.Write(message).Subscribe(writeSuccess =>
                                {
                                    characteristic.Read().Subscribe(result =>
                                    {
                                        //todo: validate data from gattservice

                                        var dataToSend = new AuthSecondMessage
                                        {
                                            //Service that the watch will later advertise
                                            Password = Guid.NewGuid().ToString(),
                                            PhoneOs = ServiceLocator.Instance.Get<IDeviceInformationService>().GetPhoneOs(),
                                            HappimeterUsername = ServiceLocator.Instance.Get<IAccountStoreService>().GetAccount().Username,
                                            HappimeterUserId = ServiceLocator.Instance.Get<IAccountStoreService>().GetAccountUserId()
                                        };
                                        var jsonToSend = Newtonsoft.Json.JsonConvert.SerializeObject(dataToSend);
                                        characteristic.Write(System.Text.Encoding.UTF8.GetBytes(jsonToSend)).Subscribe((obj) =>
                                        {
                                            //todo: savepairing information to db

                                            //Lets wait for his beacon signal
                                            ServiceLocator.Instance.Get<IBeaconWakeupService>().StartWakeupForBeacon("F0000000-0000-1000-8000-00805F9B34FB", 0, dataToSend.HappimeterUserId);
                                        });
                                    });
                                }, writeError =>
                                {
                                    Debug.WriteLine($"Got error while writing to Auth characteristic, error message {writeError.Message}");
                                });

                                characteristic.WhenNotificationReceived().Subscribe(result =>
                                {
                                    Debug.WriteLine("Got Notification: " + System.Text.Encoding.UTF8.GetString(result.Data));

                                });
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

        public void ExchangeData() {
            Device.WhenAnyCharacteristicDiscovered().Subscribe(characteristic =>
            {
                if (characteristic.Uuid == DataCharacterisic)
                {
                    Debug.WriteLine("DataCharacteristic found");
                    characteristic.Write(System.Text.Encoding.UTF8.GetBytes("pass")).Subscribe(writeResult => {
                        characteristic.Read().Subscribe(readResult => {
                            //readResult should hold the data
                            Debug.WriteLine("Read data: " + string.Concat(readResult.Data));
                        });
                    });
                }
            });
        }
    }
}
