using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Events;
using Happimeter.Interfaces;
using Happimeter.Models;
using Plugin.BluetoothLE;

namespace Happimeter.Services
{
    public class BluetoothService : IBluetoothService
    {

        private const int _messageTimeoutSeconds = 15;
        private const int _scanTimeoutSeconds = 20;

        private bool RescanEvenIfConnectedNextTime = false;

        private ReplaySubject<IScanResult> ScanReplaySubject = new ReplaySubject<IScanResult>();

        public BluetoothDevice PairedDevice { get; set; }
        public IList<IDevice> ConnectedDevices { get; private set; }
        public IList<IScanResult> FoundDevices { get; private set; }


        public const string HeartRateService = "0000180D-0000-1000-8000-00805f9b34fb";
        public const string GenericService = "00001800-0000-1000-8000-00805f9b34fb";
        public const string MiBandService = "0000FEE0-0000-1000-8000-00805f9b34fb";

        public const string NotificationHeartRate = "00002a37-0000-1000-8000-00805f9b34fb";
        public const string ControlHeartRate = "00002a39-0000-1000-8000-00805f9b34fb";
        public const string NameCharacteristic = "00002A00-0000-1000-8000-00805f9b34fb";//not discovered
        public static Guid ButtonTouch = Guid.Parse("00000010-0000-3512-2118-0009af100700");

        //public const string MiAuthCharacteristic = "00000009-0000-1000-8000-00805f9b34fb";//maybe instead : 00000009-0000-3512-2118-0009af100700
        public const string MiAuthCharacteristic = "00000009-0000-3512-2118-0009af100700";//maybe instead : 00000009-0000-3512-2118-0009af100700
        public static readonly Guid NotificationCharacteristic = Guid.Parse("00002a46-0000-1000-8000-00805f9b34fb");

        public static readonly byte[] MiBandSecret = new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45 };
        public Dictionary<Guid, Dictionary<Guid, IGattCharacteristic>> DevicesCharacteristics { get; set; }

        public BluetoothService()
        {
            ConnectedDevices = new List<IDevice>();
            FoundDevices = new List<IScanResult>();
            DevicesCharacteristics = new Dictionary<Guid, Dictionary<Guid, IGattCharacteristic>>();
        }

        public IObservable<IScanResult> StartScan(string serviceGuid = null)
        {

            if (CrossBleAdapter.Current.IsScanning || CrossBleAdapter.Current.Status != AdapterStatus.PoweredOn)
            {
                if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff)
                {
                    //reset the replaysubject
                    ScanReplaySubject = new ReplaySubject<IScanResult>();
                    ScanReplaySubject.OnCompleted();
                    //todo: open settings
                }
                return ScanReplaySubject;
            }

            IObservable<IScanResult> scannerObs;
            if (serviceGuid == null)
            {
                scannerObs = CrossBleAdapter.Current.Scan();
            }
            else
            {
                scannerObs = CrossBleAdapter.Current.Scan(new ScanConfig { ServiceUuids = new List<Guid> { Guid.Parse(serviceGuid) }, ScanType = BleScanType.LowLatency });
            }
            ScanReplaySubject = new ReplaySubject<IScanResult>();
            FoundDevices = new List<IScanResult>();

            var timerObs = Observable.Timer(TimeSpan.FromSeconds(_scanTimeoutSeconds));
            scannerObs.TakeUntil(timerObs).Subscribe(scan =>
            {
                if (!FoundDevices.Select(x => x.Device.Uuid).Contains(scan.Device.Uuid))
                {
                    Console.WriteLine($"Found device. Name: {scan.Device.Name}, Uuid: {scan.Device.Uuid}, data: {System.Text.Encoding.UTF8.GetString(scan.AdvertisementData.ServiceData?.FirstOrDefault() ?? new byte[0])}");
                    FoundDevices.Add(scan);
                    ScanReplaySubject.OnNext(scan);
                }
            });
            timerObs.Subscribe((longi) =>
            {
                ScanReplaySubject.OnCompleted();
            });
            return ScanReplaySubject;
        }



        public bool IsConnected(IDevice device)
        {
            return CrossBleAdapter.Current.GetConnectedDevices().Select(dev => dev.Uuid).Contains(device.Uuid);
        }

        public void RemoveAllConnections()
        {
            var devices = CrossBleAdapter.Current.GetConnectedDevices();
            foreach (var device in devices)
            {
                device.CancelConnection();
            }
        }

        public IObservable<bool> PairDevice(BluetoothDevice device)
        {
            device.Connect();
            var obs = device.WhenDeviceReady().Take(1);
            obs.Subscribe(success =>
            {
                PairedDevice = device;
                if (PairedDevice.Device.IsPairingAvailable() && false)
                {
                    Console.WriteLine("Pairing is available");
                    PairedDevice.Device.PairingRequest().Subscribe(result =>
                    {
                        Console.WriteLine("Paired: " + result);
                    });
                }
                PairedDevice.Device.WhenStatusChanged().Subscribe(status =>
                {
                    Console.WriteLine("Status changed: " + status);
                    if (status == ConnectionStatus.Disconnected)
                    {
                        Console.WriteLine("DEVICE DISCONNECTED");
                        //PairedDevice.Device.Connect(new GattConnectionConfig() { IsPersistent = true, AutoConnect = true, Priority = ConnectionPriority.High }); 
                    }
                });
            });

            CrossBleAdapter.Current.WhenDeviceStateRestored().Subscribe(restoredDevice =>
            {
                Console.WriteLine("RESTORED DEVICE");
                // will return the device(s) that are reconnecting
            });

            return obs;
        }

        /// <summary>
        /// This methods returns a paired (paired means it has an entry wihtin our db, not neccessary paired as per BT terminology) device.
        /// If the corresponding device is already connected to the phone, it will simply be returned.
        /// If there is no connection to the device (maybe because we went out of range or something similar) we scan for a few seconds to find the device and return it if we find it.
        /// </summary>
        /// <returns>The connected device.</returns>
        private async Task<IDevice> GetConnectedDevice()
        {
            var connectedDevices = CrossBleAdapter.Current.GetConnectedDevices();

            //todo: not be reliable on name
            if (connectedDevices.Any(x => x.Name?.Contains("Happimeter") ?? false))
            {
                var innerDevice = connectedDevices.FirstOrDefault(x => x.Name.Contains("Happimeter"));
                if (innerDevice.Status == ConnectionStatus.Connected && RescanEvenIfConnectedNextTime)
                {
                    Console.WriteLine("Device already connected");
                    return innerDevice;
                }
                RescanEvenIfConnectedNextTime = false;
            }

            if (!CrossBleAdapter.Current.IsScanning)
            {
                Console.WriteLine("Not connected, not scanning, starting scanning");
                var replaySubj = StartScan(UuidHelper.AndroidWatchServiceUuidString);
            }
            var userId = ServiceLocator.Instance.Get<IAccountStoreService>().GetAccountUserId();
            var userIdBytes = System.Text.Encoding.UTF8.GetBytes(userId.ToString());
            //we skip 2 because the first two bytes are not relevant to us. the actual advertisement data start at position 3
            var device = await ScanReplaySubject.Where(scanRes => scanRes?.AdvertisementData?.ServiceData?.FirstOrDefault()?.Skip(2)?.SequenceEqual(userIdBytes) ?? false)
                                                .Select(result => result.Device)
                                                .Take(1)
                                                .Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds))
                                                .Catch((Exception arg) =>
                                                {
                                                    Console.WriteLine(arg.Message);
                                                    return Observable.Return<IDevice>(null);
                                                })
                                                .DefaultIfEmpty();
            if (device == null)
            {
                return null;
            }

            bool connectionResult = true;
            await device.Connect(
                new GattConnectionConfig
                {
                    AutoConnect = false,
                    Priority = ConnectionPriority.High
                })
                .Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds))
                                            .Catch((Exception arg) =>
                                            {
                                                connectionResult = false;
                                                return Observable.Return<object>(null);
                                            });

            if (!connectionResult)
            {
                //connection was not successful!
                return null;
            }

            device.WhenStatusChanged().Subscribe(x =>
            {
                Debug.WriteLine($"Connection Status Changed: {x}");
            });
            return device;

        }

        public async void SendMeasurementMode(int? interval = null, Action<BluetoothWriteEvent> statusUpdate = null)
        {
            statusUpdate?.Invoke(BluetoothWriteEvent.Initialized);
            var device = await GetConnectedDevice();
            if (device == null)
            {
                //device could either not be found or not be connected to
                statusUpdate?.Invoke(BluetoothWriteEvent.ErrorOnConnectingToDevice);
                return;
            }
            statusUpdate?.Invoke(BluetoothWriteEvent.Connected);
            var characteristic = await device.WhenAnyCharacteristicDiscovered()
                                             .Where(charac => charac.Uuid == UuidHelper.MeasurementModeCharacteristicUuid)
                                             .Take(1)
                                             .Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds))
                                             .Catch((Exception e) => Observable.Return<IGattCharacteristic>(null));

            if (characteristic == null)
            {
                //we did not find the characteristic within the give timeframe
                statusUpdate?.Invoke(BluetoothWriteEvent.ErrorOnConnectingToDevice);
                return;
            }

            var message = new SwitchMeasurementModeMessage(interval);

            var result = await WriteAsync(characteristic, message);
            if (result)
            {
                statusUpdate?.Invoke(BluetoothWriteEvent.Complete);
            }
            else
            {
                statusUpdate?.Invoke(BluetoothWriteEvent.ErrorOnWrite);
            }
        }

        public async void SendGenericQuestions(Action<BluetoothWriteEvent> statusUpdate = null)
        {
            statusUpdate?.Invoke(BluetoothWriteEvent.Initialized);
            var device = await GetConnectedDevice();
            if (device == null)
            {
                //device could either not be found or not be connected to
                statusUpdate?.Invoke(BluetoothWriteEvent.ErrorOnConnectingToDevice);
                return;
            }
            statusUpdate?.Invoke(BluetoothWriteEvent.Connected);
            var characteristic = await device.WhenAnyCharacteristicDiscovered()
                                             .Where(charac => charac.Uuid == UuidHelper.GenericQuestionCharacteristicUuid)
                                             .Take(1)
                                             .Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds))
                                             .Catch((Exception e) => Observable.Return<IGattCharacteristic>(null));

            if (characteristic == null)
            {
                //we did not find the characteristic within the give timeframe
                statusUpdate?.Invoke(BluetoothWriteEvent.ErrorOnConnectingToDevice);
                return;
            }

            var genericQuestions = ServiceLocator.Instance.Get<ISharedDatabaseContext>().GetAll<GenericQuestion>();
            var genericQuestionMessage = new GenericQuestionMessage
            {
                Questions = genericQuestions
            };

            var result = await WriteAsync(characteristic, genericQuestionMessage);
            if (result)
            {
                statusUpdate?.Invoke(BluetoothWriteEvent.Complete);
            }
            else
            {
                statusUpdate?.Invoke(BluetoothWriteEvent.ErrorOnWrite);
            }
        }

        //todo: maybe use give a statusUpdate delagate to function instead of providing events
        public event EventHandler<AndroidWatchExchangeDataEventArgs> DataExchangeStatusUpdate;
        //is busy with data exchange?
        private Dictionary<Guid, bool> IsBusy = new Dictionary<Guid, bool>();
        public async void ExchangeData()
        {
            var deviceInfoServic = ServiceLocator.Instance.Get<IDeviceInformationService>();
            await deviceInfoServic.RunCodeInBackgroundMode(async () =>
            {
                DataExchangeStatusUpdate?.Invoke(this,
                                                 new AndroidWatchExchangeDataEventArgs
                                                 {
                                                     EventType = AndroidWatchExchangeDataStates.SearchingForDevice
                                                 });
                var device = await GetConnectedDevice();
                if (device == null)
                {
                    DataExchangeStatusUpdate?.Invoke(this,
                                     new AndroidWatchExchangeDataEventArgs
                                     {
                                         EventType = AndroidWatchExchangeDataStates.DeviceNotFound
                                     });
                    return;
                }

                var characteristic = await device.WhenAnyCharacteristicDiscovered()
                                                 .Where(charac => charac.Uuid == UuidHelper.DataExchangeCharacteristicUuid)
                                                 .Take(1)
                                                 .Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds))
                                                 .Catch((Exception e) =>
                                                 {
                                                     //device.CancelConnection();
                                                     RescanEvenIfConnectedNextTime = true;
                                                     return Observable.Return<IGattCharacteristic>(null);
                                                 });

                if (characteristic == null)
                {
                    DataExchangeStatusUpdate?.Invoke(this,
                                     new AndroidWatchExchangeDataEventArgs
                                     {
                                         EventType = AndroidWatchExchangeDataStates.CouldNotDiscoverCharacteristic
                                     });
                    Debug.WriteLine("Could not discover");
                    device.CancelConnection();
                    return;
                }

                CharacteristicDiscoveredForDataExchange(characteristic);
            }, "data_exchange_task");
        }

        private async void CharacteristicDiscoveredForDataExchange(IGattCharacteristic characteristic)
        {
            Console.WriteLine("Characteristic discovered: " + characteristic.Uuid);

            if (characteristic.Uuid == UuidHelper.DataExchangeCharacteristicUuid)
            {
                DataExchangeStatusUpdate?.Invoke(this,
                    new AndroidWatchExchangeDataEventArgs
                    {
                        EventType = AndroidWatchExchangeDataStates.CharacteristicDiscovered
                    });

                if (!IsBusy.ContainsKey(characteristic.Service.Device.Uuid))
                {
                    IsBusy.Add(characteristic.Service.Device.Uuid, true);//check wheter we are locked
                }
                else
                {
                    Console.WriteLine("We are still busy with an previous data exchnage, lets abort the current one.");
                    return;
                }


                var success = await WriteAsync(characteristic, new DataExchangeFirstMessage());
                Console.WriteLine("wrote successfully");
                if (!success)
                {
                    //writing was not successful
                    DataExchangeStatusUpdate?.Invoke(this,
                                 new AndroidWatchExchangeDataEventArgs
                                 {
                                     EventType = AndroidWatchExchangeDataStates.ErrorOnExchange
                                 });
                    IsBusy.Remove(characteristic.Service.Device.Uuid);
                    return;
                }

                DataExchangeStatusUpdate?.Invoke(this,
                                 new AndroidWatchExchangeDataEventArgs
                                 {
                                     EventType = AndroidWatchExchangeDataStates.DidWrite
                                 });

                //FROM HERE ON WE RUN IT IN A BACKGROUND THREAD
                try
                {
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    var result = await ReadAsync(characteristic, (read, total) =>
                    {
                        DataExchangeStatusUpdate?.Invoke(this,
                             new AndroidWatchExchangeDataEventArgs
                             {
                                 EventType = AndroidWatchExchangeDataStates.ReadUpdate,
                                 BytesRead = read,
                                 TotalBytes = total
                             });
                    });
                    if (result == null)
                    {
                        //we throw an exception and let the catch block handle it
                        throw new ArgumentException("Error during read process");
                    }
                    stopWatch.Stop();
                    Console.WriteLine($"Took {stopWatch.Elapsed.TotalSeconds} seconds to receive {result.Count()} bytes");

                    //get the read data and save it to db
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<DataExchangeMessage>(result);
                    ServiceLocator.Instance.Get<IMeasurementService>().AddMeasurements(data);

                    //update timestamp for last pairing
                    var pairing = ServiceLocator.Instance.Get<ISharedDatabaseContext>().Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);
                    pairing.LastDataSync = DateTime.UtcNow;
                    ServiceLocator.Instance.Get<ISharedDatabaseContext>().Update(pairing);

                    //inform watch that we stored his data. In turn it will delete the data on the watch.
                    await WriteAsync(characteristic, new DataExchangeConfirmationMessage());
                    //As last step we upload the data to the server 
                    await ServiceLocator.Instance.Get<IHappimeterApiService>().UploadMood();
                    await ServiceLocator.Instance.Get<IHappimeterApiService>().UploadSensor();

                    Console.WriteLine("Succesfully finished data exchange");
                    IsBusy.Remove(characteristic.Service.Device.Uuid);
                    DataExchangeStatusUpdate?.Invoke(this,
                        new AndroidWatchExchangeDataEventArgs
                        {
                            EventType = AndroidWatchExchangeDataStates.Complete,
                        });
                    var eventData = new Dictionary<string, string> {
                            {"durationSeconds", stopWatch.Elapsed.TotalSeconds.ToString()},
                            {"bytesTransfered", result.Count().ToString()}
                        };
                    ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.DataExchangeEnd, eventData);
                }
                catch (Exception e)
                {
                    DataExchangeStatusUpdate?.Invoke(this,
                        new AndroidWatchExchangeDataEventArgs
                        {
                            EventType = AndroidWatchExchangeDataStates.ErrorOnExchange,
                        });
                    Console.WriteLine($"Exception on Dataexchange after starting the exchange: {e.Message}");
                    ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.DataExchangeFailure);
                    IsBusy.Remove(characteristic.Service.Device.Uuid);
                }

            }
        }

        /// <summary>
        ///     We return false if there is an error during the write process.
        ///     We return true if the write process succeds.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="characteristic">Characteristic.</param>
        /// <param name="message">Message.</param>
        public async Task<bool> WriteAsync(IGattCharacteristic characteristic, BaseBluetoothMessage message)
        {
            //send reset bytes, to ensure, that the other side knows we are starting a new connection
            //Otherwise the other side wouldn't know, that the next message is the header.
            var nullByteSeq = new byte[3] { 0x00, 0x00, 0x00 };
            var reseted = await characteristic.Write(nullByteSeq)
                .Timeout(TimeSpan.FromSeconds(5))
                .Catch<CharacteristicResult, Exception>((arg) =>
                {
                    if (arg is TimeoutException)
                    {
                        Console.WriteLine("Writing took longer than 5 seconds. Abort!");
                    }
                    else
                    {
                        Console.WriteLine("Got error while writing reset!");
                    }
                    return Observable.Return<CharacteristicResult>(null);
                });
            if (reseted == null)
            {
                return false;
            }

            //creating and sending header.
            var messageJson = BluetoothHelper.GetMessageJson(message);
            var header = BluetoothHelper.GetMessageHeader(message);
            var written = await characteristic.Write(header)
                .Timeout(TimeSpan.FromSeconds(5))
                .Catch<CharacteristicResult, Exception>((arg) =>
                {
                    if (arg is TimeoutException)
                    {
                        Console.WriteLine("Writing took longer than 5 seconds. Abort!");
                    }
                    else
                    {
                        Console.WriteLine("Got error on write, while writing header");
                    }
                    return Observable.Return<CharacteristicResult>(null);
                });
            if (written == null)
            {
                return false;
            };

            //negotiating higher mtu would increase the transfer speed, however it makes it very instable at the moment we stick with 20 bytes which is the default
            var mtu = 20;

            var bytesSentCounter = 0;
            while (bytesSentCounter < messageJson.Count())
            {
                var toSend = messageJson.Skip(bytesSentCounter).Take(mtu).ToArray();
                var sent = await characteristic.Write(toSend)
                   .Timeout(TimeSpan.FromSeconds(8))
                   .Catch<CharacteristicResult, Exception>((arg) =>
                   {
                       if (arg is TimeoutException)
                       {
                           Console.WriteLine("Writing took longer than 8 seconds. Abort!");
                       }
                       else
                       {
                           Console.WriteLine("Got error on write, while transfering data");
                       }
                       return Observable.Return<CharacteristicResult>(null);
                   });
                if (sent == null)
                {
                    return false;
                };

                bytesSentCounter += toSend.Count();
            }
            return true;
        }

        /// <summary>
        ///     In case there is something wrong with the read, we return null.
        ///     If the read succeeds, string is the json representation of the received data.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="characteristic">Characteristic.</param>
        /// <param name="statusUpdateAction">Status update action.</param>
        public async Task<string> ReadAsync(IGattCharacteristic characteristic, Action<int, int> statusUpdateAction = null)
        {

            //from the first read request we assume to get an header, which contains information what and how much is sent
            var headerResult = await characteristic.Read()
                    .Timeout(TimeSpan.FromSeconds(10))
                    .Catch<CharacteristicResult, Exception>((arg) =>
                    {
                        if (arg is TimeoutException)
                        {
                            Console.WriteLine("Reading took longer than 5 seconds. Abort!");
                        }
                        else
                        {
                            Console.WriteLine("Got error while reading header!");
                        }
                        return Observable.Return<CharacteristicResult>(null);
                    });
            if (headerResult == null)
            {
                return null;
            }
            var context = new WriteReceiverContext(headerResult.Data);

            while (!context.ReadComplete)
            {
                //here we receive the actual data until we got the complete message
                var nextBytes = await characteristic.Read()
                    .Timeout(TimeSpan.FromSeconds(10))
                    .Catch<CharacteristicResult, Exception>((arg) =>
                    {
                        if (arg is TimeoutException)
                        {
                            Console.WriteLine("Reading took longer than 10 seconds. Abort!");
                        }
                        else
                        {
                            Console.WriteLine("Got error on reading data!");
                        }
                        return Observable.Return<CharacteristicResult>(null);
                    });
                if (nextBytes == null || nextBytes.Data.Length == 3 && nextBytes.Data.All(x => x == 0x00))
                {
                    //if next bytes are null, the read process throw an exception
                    //if we receive three zero bytes. the other side got an error
                    return null;
                }

                context.AddMessagePart(nextBytes.Data);

                if (statusUpdateAction != null)
                {
                    statusUpdateAction(context.Cursor, context.MessageSize);
                }
            }
            var returnJson = context.GetMessageAsJson();
            return returnJson;
        }

        public async Task<string> AwaitNotificationAsync(IGattCharacteristic characteristic)
        {
            if (!CharacteristicNotifiationSubjects.ContainsKey(characteristic.Uuid.ToString()))
            {
                await EnableNotificationsFor(characteristic);
            }
            var notificationSubject = CharacteristicNotifiationSubjects[characteristic.Uuid.ToString()] as IObservable<CharacteristicResult>;
            var headerNotificationResult = await notificationSubject.FirstAsync();
            var context = new WriteReceiverContext(headerNotificationResult.Data);
            while (true)
            {
                var messagePart = await notificationSubject;
                if (!context.CanAddMessagePart(messagePart.Data))
                {
                    return null;
                }
                context.AddMessagePart(messagePart.Data);
                if (context.ReadComplete)
                {
                    break;
                }
            }
            var subject = CharacteristicNotifiationSubjects[characteristic.Uuid.ToString()];
            subject.OnCompleted();
            CharacteristicNotifiationSubjects.Remove(characteristic.Uuid.ToString());

            return context.GetMessageAsJson();
        }

        private Dictionary<string, ReplaySubject<CharacteristicResult>> CharacteristicNotifiationSubjects = new Dictionary<string, ReplaySubject<CharacteristicResult>>();
        //first string is message name, second string is message json
        private ReplaySubject<(string, string)> NotificationSubject = new ReplaySubject<(string, string)>(TimeSpan.FromSeconds(2));
        public IObservable<(string, string)> WhenNotificationReceived()
        {
            return NotificationSubject as IObservable<(string, string)>;
        }

        public async Task EnableNotificationsFor(IGattCharacteristic characteristic)
        {
            await Task.Delay(1000);
            var res = await characteristic.EnableNotifications().Catch((Exception arg) =>
            {
                Console.WriteLine(arg.Message);
                return Observable.Return<bool>(true);
            });
            if (!res)
            {
                throw new Exception("Could not enable Notifications");
            }
            WriteReceiverContext context = null;
            characteristic.WhenNotificationReceived().Subscribe(result =>
            {
                if (result.Data == null)
                {
                    return;
                }
                if (context == null)
                {
                    context = new WriteReceiverContext(result.Data);
                    return;
                }
                if (!context.CanAddMessagePart(result.Data))
                {
                    context = null;
                    return;
                }
                context.AddMessagePart(result.Data);
                if (context.ReadComplete)
                {
                    var json = context.GetMessageAsJson();
                    var name = context.MessageName;
                    context = null;
                    NotificationSubject.OnNext((name, json));
                }
            });
        }
    }
}
