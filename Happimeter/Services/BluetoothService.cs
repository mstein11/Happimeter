using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

        private const int _messageTimeoutSeconds = 5;
        private const int _scanTimeoutSeconds = 10;

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

        public static readonly byte[] MiBandSecret = new byte[] {0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45};
        public Dictionary<Guid, Dictionary<Guid, IGattCharacteristic>> DevicesCharacteristics { get; set; }

        public BluetoothService()
        {
            ConnectedDevices = new List<IDevice>();
            FoundDevices = new List<IScanResult>();
            DevicesCharacteristics = new Dictionary<Guid, Dictionary<Guid, IGattCharacteristic>>();
        }

        public IObservable<IScanResult> StartScan(string serviceGuid = null) {

            if (CrossBleAdapter.Current.IsScanning || CrossBleAdapter.Current.Status != AdapterStatus.PoweredOn) {
                if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff) {
                    //reset the replaysubject
                    ScanReplaySubject = new ReplaySubject<IScanResult>();
                    //todo: open settings
                }
                return ScanReplaySubject;
            }

            IObservable<IScanResult> scannerObs;
            if (serviceGuid == null) {
                scannerObs = CrossBleAdapter.Current.Scan();
            } else {
                scannerObs = CrossBleAdapter.Current.Scan(new ScanConfig {ServiceUuids = new List<Guid> {Guid.Parse(serviceGuid)}});    
            }
            ScanReplaySubject = new ReplaySubject<IScanResult>();
            FoundDevices = new List<IScanResult>();
            scannerObs.TakeUntil(Observable.Timer(TimeSpan.FromSeconds(_scanTimeoutSeconds))).Subscribe(scan => {
                if (!FoundDevices.Select(x => x.Device.Uuid).Contains(scan.Device.Uuid)) {
                    Console.WriteLine($"Found device. Name: {scan.Device.Name}, Uuid: {scan.Device.Uuid}, data: {System.Text.Encoding.UTF8.GetString(scan.AdvertisementData.ServiceData?.FirstOrDefault() ?? new byte[0])}");
                    FoundDevices.Add(scan);
                    ScanReplaySubject.OnNext(scan);
                }
            });
            return ScanReplaySubject;
        }



        public bool IsConnected(IDevice device) {
            return CrossBleAdapter.Current.GetConnectedDevices().Select(dev => dev.Uuid).Contains(device.Uuid);
        }

        public IObservable<bool> PairDevice(BluetoothDevice device) {
            device.Connect();
            var obs = device.WhenDeviceReady();
            obs.Subscribe(success => {
                PairedDevice = device;
                if(PairedDevice.Device.IsPairingAvailable()) {
                    Console.WriteLine("Pairing is available");
                    PairedDevice.Device.PairingRequest().Subscribe(result => {
                        Console.WriteLine("Paired: " + result);
                    });
                }
                PairedDevice.Device.WhenStatusChanged().Subscribe(status => {
                    Console.WriteLine("Status changed: " + status);
                    if(status == ConnectionStatus.Disconnected) {
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

        public event EventHandler<AndroidWatchExchangeDataEventArgs> DataExchangeStatusUpdate;

        //is busy with data exchange?
        private Dictionary<Guid, bool> IsBusy = new Dictionary<Guid, bool>();

        public void ExchangeData() {
            Console.WriteLine("Starting ExchangeData");
            var connectedDevices = CrossBleAdapter.Current.GetConnectedDevices();
            var pairedDevices = CrossBleAdapter.Current.GetPairedDevices();
            var userId = ServiceLocator.Instance.Get<IAccountStoreService>().GetAccountUserId();
            var userIdBytes = System.Text.Encoding.UTF8.GetBytes(userId.ToString());

            DataExchangeStatusUpdate?.Invoke(this,
                                             new AndroidWatchExchangeDataEventArgs
                                             {
                                                 EventType = AndroidWatchExchangeDataStates.SearchingForDevice
                                             });

            if (connectedDevices.Any(x => x.Name?.Contains("Happimeter") ?? false))
            {
                DataExchangeStatusUpdate?.Invoke(this,
                                 new AndroidWatchExchangeDataEventArgs
                                 {
                                     EventType = AndroidWatchExchangeDataStates.DeviceConnected
                                 });
                Console.WriteLine("Device already connected");
                connectedDevices.FirstOrDefault(x => x.Name.Contains("Happimeter"))
                                .WhenAnyCharacteristicDiscovered()
                                .Where(characteristic => characteristic.Uuid == UuidHelper.DataExchangeCharacteristicUuid)
                                .Take(1)
                                .Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds))
                                .Subscribe(CharacteristicDiscoveredForDataExchange, CancelConnectionOnTimeoutError);
                
            } else {
                if (!CrossBleAdapter.Current.IsScanning) {
                    Console.WriteLine("Not connected, not scanning, starting scanning");
                    StartScan(UuidHelper.AndroidWatchServiceUuidString);
                }

                //we skip 2 because the first two bytes are not relevant to us. the actual advertisement data start at position 3
                ScanReplaySubject.Where(scanRes => scanRes?.AdvertisementData?.ServiceData?.FirstOrDefault()?.Skip(2)?.SequenceEqual(userIdBytes) ?? false).Select(result => result.Device)
                                 .Take(1)
                                 .Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds))
                                 .Subscribe(result =>
                {
                    Console.WriteLine("Found matching device");
                    result.Connect()
                          .Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds))
                          .Subscribe(conn =>
                    {
                        DataExchangeStatusUpdate?.Invoke(this,
                                 new AndroidWatchExchangeDataEventArgs
                                 {
                                     EventType = AndroidWatchExchangeDataStates.DeviceConnected
                                 });

                        Console.WriteLine("Connected");
                        result.WhenAnyCharacteristicDiscovered()
                              .Where(characteristic => characteristic.Uuid == UuidHelper.DataExchangeCharacteristicUuid)
                                .Take(1)
                              .Timeout(TimeSpan.FromSeconds(_messageTimeoutSeconds)) 
                                .Subscribe(CharacteristicDiscoveredForDataExchange, CancelConnectionOnTimeoutError);
                    }, err => //Error from connecting to device
                    {
                        DataExchangeStatusUpdate?.Invoke(this,
                                 new AndroidWatchExchangeDataEventArgs
                                 {
                                     EventType = AndroidWatchExchangeDataStates.DeviceNotFound
                                 });
                        Console.WriteLine($"Failed to connect to device: {result.Uuid}");
                    });

                }, err => {//Error from Replaysubject (scan)
                    if (err is TimeoutException) {
                        Console.WriteLine($"No device found in {_messageTimeoutSeconds} seconds");
                        return;
                    }
                    DataExchangeStatusUpdate?.Invoke(this,
                                 new AndroidWatchExchangeDataEventArgs
                                 {
                                     EventType = AndroidWatchExchangeDataStates.CouldNotConnect
                                 });
                    Console.WriteLine($"Received unknown exception on exchange data attempt: {err.Message}");
                });
            }
        }

        private void CancelConnectionOnTimeoutError(Exception e) {
            DataExchangeStatusUpdate?.Invoke(this,
             new AndroidWatchExchangeDataEventArgs
             {
                 EventType = AndroidWatchExchangeDataStates.CouldNotDiscoverCharacteristic
             });
            if (e is TimeoutException)
            {
                Console.WriteLine("Timeout while waiting for characteristic on already connected device!");
                var devicesToDisconnect = CrossBleAdapter.Current.GetConnectedDevices();
                foreach (var device in devicesToDisconnect)
                {
                    device.CancelConnection();
                    Console.WriteLine($"Cancelled Connection to device {device.Uuid}");
                }
                //restart!
                ExchangeData();
            }
        }

        private void CharacteristicDiscoveredForDataExchange(IGattCharacteristic characteristic) {
            Console.WriteLine("Characteristic discovered: " + characteristic.Uuid);

            if (characteristic.Uuid == UuidHelper.DataExchangeCharacteristicUuid)
            {
                DataExchangeStatusUpdate?.Invoke(this,
                                 new AndroidWatchExchangeDataEventArgs
                                 {
                                     EventType = AndroidWatchExchangeDataStates.CharacteristicDiscovered
                                 });

                if (!IsBusy.ContainsKey(characteristic.Service.Device.Uuid)) {
                    IsBusy.Add(characteristic.Service.Device.Uuid, true);//check wheter we are locked
                } else {
                    Console.WriteLine("We are still busy with an previous data exchnage, lets abort the current one.");
                    return;
                }

                //datacharacteristic
                characteristic.Write(new DataExchangeFirstMessage().GetAsBytes())
                              .Timeout(TimeSpan.FromSeconds(5))
                              .Subscribe(async writeResult =>
                {
                    DataExchangeStatusUpdate?.Invoke(this,
                                 new AndroidWatchExchangeDataEventArgs
                                 {
                                     EventType = AndroidWatchExchangeDataStates.DidWrite
                                 });
                    try {
                        Console.WriteLine("wrote successfully");
                        var listOfBytes = new List<byte>();
                        var totalBytesRead = 0;
                        var stopWatch = new Stopwatch();
                        stopWatch.Start();
                        while (true)
                        {
                            var result = await characteristic.Read();
                            if (result == null || result.Data == null || result.Data.Length == 1)
                            {
                                break;
                            }
                            Console.WriteLine($"Got {result.Data.Length} bytes.");
                            listOfBytes.AddRange(result.Data);
                            totalBytesRead += result.Data.Length;
                            DataExchangeStatusUpdate?.Invoke(this,
                                 new AndroidWatchExchangeDataEventArgs
                                 {
                                     EventType = AndroidWatchExchangeDataStates.ReadUpdate,
                                     BytesRead = totalBytesRead,
                                     TotalBytes = 100
                                 });
                            if (result.Data.Length == 1)
                            {
                                break;
                            }
                        }
                        stopWatch.Stop();
                        var json = System.Text.Encoding.UTF8.GetString(listOfBytes.ToArray());
                        Console.WriteLine($"Took {stopWatch.Elapsed.TotalSeconds} seconds to receive {totalBytesRead} bytes");
                        Console.WriteLine($"Received Message: {json}");

                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<DataExchangeMessage>(json);
                        ServiceLocator.Instance.Get<IMeasurementService>().AddMeasurements(data);

                        var pairing = ServiceLocator.Instance.Get<ISharedDatabaseContext>().Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);
                        pairing.LastDataSync = DateTime.UtcNow;
                        ServiceLocator.Instance.Get<ISharedDatabaseContext>().Update(pairing);

                        await characteristic.Write(new DataExchangeConfirmationMessage().GetAsBytes());
                        Console.WriteLine("Succesfully finished data exchange");
                        IsBusy.Remove(characteristic.Service.Device.Uuid);
                        DataExchangeStatusUpdate?.Invoke(this,
                                 new AndroidWatchExchangeDataEventArgs
                                 {
                                     EventType = AndroidWatchExchangeDataStates.Complete,
                                     BytesRead = totalBytesRead,
                                     TotalBytes = 100
                                 });

                    } catch (Exception e) {
                        DataExchangeStatusUpdate?.Invoke(this,
                             new AndroidWatchExchangeDataEventArgs
                             {
                                 EventType = AndroidWatchExchangeDataStates.ErrorOnExchange,
                             });
                        Console.WriteLine($"Exception on Dataexchange after starting the exchange: {e.Message}");
                        IsBusy.Remove(characteristic.Service.Device.Uuid);    
                    }
                }, (Exception e) => {
                    CancelConnectionOnTimeoutError(e);
                    IsBusy.Remove(characteristic.Service.Device.Uuid);
                });
            }
        }
    }
}
