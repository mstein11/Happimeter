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
        WaitingForNotification,
        ErrorOnFirstWrite,
        UserAccepted,
        UserDeclined,
        NotificationTimeout,
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
                            await btService.EnableNotificationsFor(characteristic);
                            var deviceName = ServiceLocator.Instance.Get<IDeviceInformationService>().GetDeviceName();

                            var writeResult = await btService.WriteAsync(characteristic, new AuthFirstMessage(deviceName));
                            if (!writeResult) {
                                //we got an error here
                                OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.ErrorOnFirstWrite, null);
                                return;
                            }
                            OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.WaitingForNotification, null);

                            //item one is message name item two  is message json
                            var notification = await btService.WhenNotificationReceived()
                                                              .Where((arg) => arg.Item1 == AuthNotificationMessage.MessageNameConstant)
                                                              .Timeout(TimeSpan.FromMinutes(1))
                                                              .Take(1)
                                                              .Catch<(string, string), Exception>((arg) =>
                                                              {
                                                                  return Observable.Return<(string, string)>((null, null));
                                                              });

                            if (notification.Item2 == null)
                            {
                                OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.NotificationTimeout, null);
                                return;
                            }
                            var message = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthNotificationMessage>(notification.Item2);
                            if(!message.Accepted) {
                                OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.UserDeclined, null);
                                return;
                            }
                            OnConnectingStateChanged?.Invoke(AndroidWatchConnectingStates.UserAccepted, null);
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
