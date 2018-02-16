using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Interfaces;
using Happimeter.Models;
using Plugin.BluetoothLE;

namespace Happimeter.Services
{
    public class BluetoothService : IBluetoothService
    {

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
                if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff && CrossBleAdapter.Current.CanOpenSettings()) {
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

            scannerObs.TakeUntil(Observable.Timer(TimeSpan.FromSeconds(10))).Subscribe(scan => {
                if (!FoundDevices.Select(x => x.Device.Uuid).Contains(scan.Device.Uuid)) {
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

        public void ExchangeData() {
            Console.WriteLine("Starting ExchangeData");
            var connectedDevices = CrossBleAdapter.Current.GetConnectedDevices();
            var pairedDevices = CrossBleAdapter.Current.GetPairedDevices();
            var userId = ServiceLocator.Instance.Get<IAccountStoreService>().GetAccountUserId();
            var userIdBytes = System.Text.Encoding.UTF8.GetBytes(userId.ToString());


            if (connectedDevices.Any(x => x.Name?.Contains("Happimeter") ?? false))
            {
                Console.WriteLine("Device already connected");
                connectedDevices.FirstOrDefault(x => x.Name.Contains("Happimeter"))
                                .WhenAnyCharacteristicDiscovered()
                                .Where(characteristic => characteristic.Uuid == Guid.Parse("7918ec07-2ba4-4542-aa13-0a10ff3826ba"))
                                .Take(1)
                                .Timeout(TimeSpan.FromSeconds(10))
                                .Subscribe(CharacteristicDiscoveredForDataExchange, err => {
                    if (err is TimeoutException) {
                        Console.WriteLine("Timeout while waiting for characteristic on already connected device!");
                        var devicesToDisconnect = CrossBleAdapter.Current.GetConnectedDevices();
                        foreach (var device in devicesToDisconnect) {
                            device.CancelConnection();
                            Console.WriteLine($"Cancelled Connection to device {device.Uuid}");
                        }
                    }
                });
                
            } else {
                if (!CrossBleAdapter.Current.IsScanning) {
                    Console.WriteLine("Not connected, not scanning, starting scanning");
                    StartScan(UuidHelper.DataExchangeCharacteristicUuidString);
                }

                //todo: exchange hardcoded name in filter with advertised data
                ScanReplaySubject.Where(scanRes => scanRes?.AdvertisementData?.ServiceData?.FirstOrDefault()?.Skip(2)?.SequenceEqual(userIdBytes) ?? false).Select(result => result.Device)
                                 .Take(1)
                                 .Timeout(TimeSpan.FromSeconds(10))
                                 .Subscribe(result =>
                {
                    Console.WriteLine("Subscribed to device");
                    result.Connect().Subscribe(conn =>
                    {
                        Console.WriteLine("Connected");
                        result.WhenAnyCharacteristicDiscovered()
                                                .Where(characteristic => characteristic.Uuid == Guid.Parse("7918ec07-2ba4-4542-aa13-0a10ff3826ba"))
                                                .Take(1)
                                                .Subscribe(CharacteristicDiscoveredForDataExchange);
                    }, err =>
                    {
                        Console.WriteLine("Err");
                    });

                }, err => {
                    if (err is TimeoutException) {
                        Console.WriteLine("No device found in 10 seconds");
                        return;
                    }
                    Console.WriteLine($"Received unknown exception on exchange data attempt: {err.Message}");
                });
            }
        }

        private Dictionary<Guid, bool> IsBusy = new Dictionary<Guid, bool>();

        private async void CharacteristicDiscoveredForDataExchange(IGattCharacteristic characteristic) {
            Console.WriteLine("Characteristic discovered: " + characteristic.Uuid);


            if (characteristic.Uuid == Guid.Parse("7918ec07-2ba4-4542-aa13-0a10ff3826ba"))
            {
                if (!IsBusy.ContainsKey(characteristic.Service.Device.Uuid)) {
                    IsBusy.Add(characteristic.Service.Device.Uuid, true);
                } else {
                    Console.WriteLine("We are still busy with an previous data exchnage, lets abourt the current one.");
                    return;
                }
                try
                {
                    //datacharacteristic
                    characteristic.Write(new DataExchangeFirstMessage().GetAsBytes()).Subscribe(async writeResult =>
                    {

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

                    });
                } catch (Exception e) {
                    Console.WriteLine("Error during data exchange");
                } finally {
                    IsBusy.Remove(characteristic.Service.Device.Uuid);
                }
            }
        }
    }
}
