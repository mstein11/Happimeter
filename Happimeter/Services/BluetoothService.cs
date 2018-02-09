using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Threading.Tasks;
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

            IObservable<IScanResult> scannerObs;
            if (serviceGuid == null) {
                scannerObs = CrossBleAdapter.Current.ScanWhenAdapterReady();    
            } else {
                scannerObs = CrossBleAdapter.Current.Scan(new ScanConfig {ServiceUuids = new List<Guid> {Guid.Parse(serviceGuid)}});    
            }


            scannerObs.TakeUntil(Observable.Timer(TimeSpan.FromSeconds(10))).Subscribe(scan => {
                //Console.WriteLine("Scan result: " + string.Concat(scan.AdvertisementData.ServiceUuids ?? ""));
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

            if (connectedDevices.Any(x => x.Name?.Contains("Happimeter") ?? false))
            {
                Console.WriteLine("Device already connected");
                connectedDevices.FirstOrDefault(x => x.Name.Contains("Happimeter"))
                                .WhenAnyCharacteristicDiscovered()
                                .Where(characteristic => characteristic.Uuid == Guid.Parse("7918ec07-2ba4-4542-aa13-0a10ff3826ba"))
                                .Take(1)
                                .Subscribe(CharacteristicDiscoveredForDataExchange);
                
            } else {
                if (!CrossBleAdapter.Current.IsScanning) {
                    Console.WriteLine("Not connected, not scanning, starting scanning");
                    StartScan("0000F0F0-0000-1000-8000-00805F9B34FB");
                }

                //ScanReplaySubject.Select(result => result.Device).Where(x => x.Name?.Contains("Happimeter") ?? false).Take(1).Subscribe(result =>
                ScanReplaySubject.Select(result => result.Device).Take(1).Subscribe(result =>
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
                    //var mtuSize = await characteristic.Service.Device.RequestMtu(256);
                    //mtuSize = await characteristic.Service.Device.RequestMtu(512);

                    //datacharacteristic
                    characteristic.Write(System.Text.Encoding.UTF8.GetBytes("pass")).Subscribe(async writeResult =>
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
                        Console.WriteLine($"Took {stopWatch.Elapsed.TotalSeconds} seconds to receive {totalBytesRead} bytes");
                        //characteristic.Service.Device.CancelConnection();
                        //Console.WriteLine("cancelled connection");
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
