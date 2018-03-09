using System;
using System.Diagnostics;
using System.Reactive.Linq;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Interfaces;
using Happimeter.Services;
using Plugin.BluetoothLE;

namespace Happimeter.Models
{
    public enum AndroidWatchConnectingStates
    {
        BtConnected,
        ErrorOnBtConnection,
        AuthCharacteristicDiscovered,
        ErrorOnAuthCharacteristicDiscovered,
        FirstWriteSuccessfull,
        ErrorOnFirstWrite,
        ReadSuccessfull,
        ErrorOnRead,
        SecondWriteSuccessfull,
        ErrorOnSecondWrite,
        Complete,
        ErrorBeforeComplete
    }

    public class AndroidWatch : BluetoothDevice
    {
        public AndroidWatch(IDevice device) : base(device) 
        {
            
        }

        public static readonly Guid ServiceUuid = Guid.Parse("2f234454-cf6d-4a0f-adf2-f4911ba9ffa6");//maybe instead : 00000009-0000-3512-2118-0009af100700
        public static readonly Guid AuthCharacteristic = Guid.Parse("68b13553-0c4d-43de-8c1c-2b10d77d2d90");

        public event EventHandler OnConnectingStateChanged;

        public override IObservable<object> Connect()
        {
            var connection = base.Connect();

            WhenDeviceReady().Take(1).Subscribe(success =>
            {
                OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.BtConnected, null);
                if (success) {
                    try {
                        Device.WhenAnyCharacteristicDiscovered().Where(x => x.Uuid == AuthCharacteristic).Take(1).Subscribe(async characteristic =>
                        {
                            Debug.WriteLine("Found our AuthCharacteristic");
                            OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.AuthCharacteristicDiscovered, null);

                            var btService = ServiceLocator.Instance.Get<IBluetoothService>();
                            var writeResult = await btService.WriteAsync(characteristic, new AuthFirstMessage());
                            if (!writeResult) {
                                //we got an error here
                                OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.ErrorOnFirstWrite, null);
                                return;
                            }
                            OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.FirstWriteSuccessfull, null);

                            var readResult = await btService.ReadAsync(characteristic);
                            if (readResult == null) {
                                //we got an error here!
                                OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.ErrorOnRead, null);
                                return;
                            }

                            OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.ReadSuccessfull, null);
                            //todo: validate data from gattservice

                            var dataToSend = new AuthSecondMessage
                            {
                                //Service that the watch will later advertise
                                Password = Guid.NewGuid().ToString(),
                                PhoneOs = ServiceLocator.Instance.Get<IDeviceInformationService>().GetPhoneOs(),
                                HappimeterUserId = ServiceLocator.Instance.Get<IAccountStoreService>().GetAccountUserId()
                            };


                            var writeResult2 = await btService.WriteAsync(characteristic, dataToSend);
                            if (!writeResult2) {
                                //we got an error here!
                                OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.ErrorOnSecondWrite, null);
                                return;
                            }

                            OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.SecondWriteSuccessfull, null);
                            var paring = new SharedBluetoothDevicePairing
                            {
                                IsPairingActive = true,
                                PairedAt = DateTime.UtcNow,
                                PairedDeviceName = characteristic.Service.Device.Name,
                                Password = dataToSend.Password,
                                PhoneOs = "Android"
                            };
                            ServiceLocator.Instance.Get<ISharedDatabaseContext>().Add(paring);

                            //Lets wait for his beacon signal
                            ServiceLocator.Instance.Get<IBeaconWakeupService>().StartWakeupForBeacon();
                            OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.Complete, null);
                            ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.PairEvent);

                            //not really needed but we leave it here incase we want to implement notifications
                            characteristic.WhenNotificationReceived().Take(1).Subscribe(result =>
                            {
                                Debug.WriteLine("Got Notification: " + System.Text.Encoding.UTF8.GetString(result.Data));
                            });
                        });
                    } catch (Exception e) {
                        Console.WriteLine("Something went wrong during authentication. Error: " + e.Message);
                        ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.PairFailureEvent);
                        OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.ErrorBeforeComplete, null);
                    }
                }
            }, error => {
                OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.ErrorOnBtConnection, null);
            });

            return connection;
        }

        public void ExchangeData() {
            Device.WhenAnyCharacteristicDiscovered().Subscribe(characteristic =>
            {
                if (characteristic.Uuid == UuidHelper.DataExchangeCharacteristicUuid)
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
