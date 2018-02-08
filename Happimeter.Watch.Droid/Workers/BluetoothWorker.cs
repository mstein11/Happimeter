using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using AltBeaconOrg.BoundBeacon;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.OS;
using Happimeter.Watch.Droid.Bluetooth;
using Happimeter.Watch.Droid.Database;
using Java.Util;
using Plugin.BluetoothLE;
using Plugin.BluetoothLE.Server;

namespace Happimeter.Watch.Droid.Workers
{
    public class BluetoothWorker : AbstractWorker
    {
        public const string BeaconUuid = "F0000000-0000-1000-8000-00805F9B34FB";
        public const string Major = "0";
        public const string Minor = "1";
        //Code that represents apple
        private const byte ManufacturerCode = 0x004C;
        private const int TxPowerLevel = -56;
        //Layout for iBeacon
        private const string BeaconLayout = "m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24";

        public BluetoothManager Manager;
        public BluetoothGattServer GattServer;

        public Dictionary<string, BluetoothDevice> SubscribedDevices = new Dictionary<string, BluetoothDevice>();

        public BluetoothWorker()
        {
            Manager = (BluetoothManager)Application.Context.GetSystemService(Android.Content.Context.BluetoothService);
        }

        public override void Start()
        {
            IsRunning = true;
            InitializeGatt();
            System.Diagnostics.Debug.WriteLine("Started Bluetooth Worker");
            var pairing = ServiceLocator.Instance.Get<IDatabaseContext>().GetCurrentBluetoothPairing();
            if (pairing == null) {
                System.Diagnostics.Debug.WriteLine("No pairing found, we start in Auth mode");
                RunAuthAdvertiser();
            } else {
                System.Diagnostics.Debug.WriteLine("Found pairing, we start in beacon mode");
                RunBeacon();
            }
        }

        private void InitializeGatt() {
            if (GattServer != null) {
                GattServer.Close();
            }
            GattServer = Manager.OpenGattServer(Application.Context, new CallbackGatt(this));
            GattServer.AddService(HappimeterService.Create());
            System.Diagnostics.Debug.WriteLine("Gatt initialized");
        }

        public override void Stop()
        {
            IsRunning = false;
        }

        private static BluetoothWorker Instance { get; set; }

        public static BluetoothWorker GetInstance()
        {
            if (Instance == null)
            {
                Instance = new BluetoothWorker();
            }

            return Instance;
        }

        /// <summary>
        ///     Every minute run for a minute a beacon that wakes up ios phones
        /// </summary>
        /// <returns>The beacon.</returns>
        public async Task RunBeacon()
        {

            ConnectableAdvertisement();
            var uuid = Encoding.UTF8.GetBytes(BeaconUuid);
            var beacon = new Beacon.Builder()
                                   .SetId1(BeaconUuid)
                                   .SetId2(Major)
                                   .SetId3(Minor)
                                   .SetManufacturer(ManufacturerCode) // Radius Networks.0x0118  Change this for other beacon layouts//0x004C for iPhone
                                   .SetTxPower(TxPowerLevel) // Power in dB
                                                             //.SetBluetoothName("Happimeter AAAA")
                                   .Build();
            var beaconParser = new BeaconParser().SetBeaconLayout(BeaconLayout);
            var trans = new BeaconTransmitter(Application.Context, beaconParser);

            while(IsRunning) {
                
                System.Diagnostics.Debug.WriteLine("About to start beacon");
                trans.StartAdvertising(beacon, new CallbackAd());
                System.Diagnostics.Debug.WriteLine("Started Beacon");
                await Task.Delay(TimeSpan.FromMinutes(25));
                trans.StopAdvertising();
                System.Diagnostics.Debug.WriteLine("Stopped Beacon");
                await Task.Delay(TimeSpan.FromMinutes(25));
            }
        }

        private async Task RunAuthAdvertiser()
        {
            System.Diagnostics.Debug.WriteLine("About to start auth advertiser");
            BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser.StopAdvertising(new CallbackAd());
            var settings = new AdvertiseSettings.Builder()
                                                .SetAdvertiseMode(AdvertiseMode.LowLatency)
                                                .SetTxPowerLevel(AdvertiseTx.PowerHigh)
                                                .SetConnectable(true)
                                                .Build();

            var tmpr = BluetoothAdapter.DefaultAdapter.SetName("Happimeter AAAA");
            var data = new AdvertiseData.Builder()
                                        .SetIncludeDeviceName(true)
                                        .SetIncludeTxPowerLevel(true)
                                        //.AddServiceUuid(ParcelUuid.FromString(BeaconUuid))
                                        //todo: add appropriate serviceId
                                        .AddServiceUuid(ParcelUuid.FromString("0000F0F0-0000-1000-8000-00805F9B34FB"))
                                        .Build();

            BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser.StartAdvertising(settings, data, new CallbackAd());
            System.Diagnostics.Debug.WriteLine("stopped auth advertiser");
        }

        private void ConnectableAdvertisement()
        {
            System.Diagnostics.Debug.WriteLine("About to start auth advertiser");
            BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser.StopAdvertising(new CallbackAd());
            var settings = new AdvertiseSettings.Builder()
                                                .SetAdvertiseMode(AdvertiseMode.LowLatency)
                                                .SetTxPowerLevel(AdvertiseTx.PowerHigh)
                                                .SetConnectable(true)
                                                .Build();

            var tmpr = BluetoothAdapter.DefaultAdapter.SetName("Happimeter AAAA");
            var data = new AdvertiseData.Builder()
                                        .SetIncludeDeviceName(true)
                                        .SetIncludeTxPowerLevel(true)
                                        //.AddServiceUuid(ParcelUuid.FromString(BeaconUuid))
                                        //todo: add appropriate serviceId
                                        .AddServiceUuid(ParcelUuid.FromString("0000F0F0-0000-1000-8000-00805F9B34FB"))
                                        //.AddServiceData(ParcelUuid.FromString("00000000-0000-1000-8000-00805F9B34FB"), Encoding.UTF8.GetBytes("Hallo"))
                                        .Build();

            BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser.StartAdvertising(settings, data, new CallbackAd());
            System.Diagnostics.Debug.WriteLine("stopped auth advertiser");
        }
    }



    public class CallbackAd : AdvertiseCallback
    {
        public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
        {
            base.OnStartSuccess(settingsInEffect);
            System.Diagnostics.Debug.WriteLine("Started advertising");
        }

        public override void OnStartFailure(AdvertiseFailure errorCode)
        {
            base.OnStartFailure(errorCode);
            System.Diagnostics.Debug.WriteLine("advertising error  " + errorCode.ToString());
        }
    }

    public class CallbackGatt : BluetoothGattServerCallback
    {
        public BluetoothWorker Worker { get; set; }

        private Dictionary<string, bool> AuthenticationDeviceDidGreat = new Dictionary<string, bool>();

        public CallbackGatt(BluetoothWorker worker)
        {
            Worker = worker;
        }

        public override void OnCharacteristicReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattCharacteristic characteristic)
        {
            base.OnCharacteristicReadRequest(device, requestId, offset, characteristic);

            var authCharac = characteristic as HappimeterAuthCharacteristic;
            if (authCharac != null) {
                authCharac.HandleRead(device,requestId,offset,Worker);
            }


            var dataCharac = characteristic as HappimeterDataCharacteristic;
            if (dataCharac != null)
            {
                dataCharac.HandleRead(device, requestId, offset, Worker);
            }
        }

        public override void OnCharacteristicWriteRequest(BluetoothDevice device, int requestId, BluetoothGattCharacteristic characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
        {
            base.OnCharacteristicWriteRequest(device, requestId, characteristic, preparedWrite, responseNeeded, offset, value);

            var authCharac = characteristic as HappimeterAuthCharacteristic;
            if (authCharac != null)
            {
                authCharac.HandleWrite(device, requestId, preparedWrite, responseNeeded, offset, value, Worker);
            }

            var dataCharac = characteristic as HappimeterDataCharacteristic;
            if (dataCharac != null)
            {
                dataCharac.HandleWriteAsync(device, requestId, preparedWrite, responseNeeded, offset, value, Worker);
            }
        }

        public override void OnDescriptorReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattDescriptor descriptor)
        {
            base.OnDescriptorReadRequest(device, requestId, offset, descriptor);

            if (descriptor.Uuid.ToString() == UUID.FromString("00002902-0000-1000-8000-00805f9b34fb").ToString())
            {
                List<byte> valueToReturn;
                if (Worker.SubscribedDevices.ContainsKey(device.Address)) {
                    valueToReturn = BluetoothGattDescriptor.EnableNotificationValue.ToList<byte>();
                } else {
                    valueToReturn = BluetoothGattDescriptor.DisableNotificationValue.ToList<byte>();
                }
                Worker.GattServer.SendResponse(device,
                    requestId,
                                               Android.Bluetooth.GattStatus.Failure,
                    0,
                    valueToReturn.ToArray());
            }
        }


        public override void OnDescriptorWriteRequest(BluetoothDevice device, int requestId, BluetoothGattDescriptor descriptor, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
        {
            base.OnDescriptorWriteRequest(device, requestId, descriptor, preparedWrite, responseNeeded, offset, value);

            if (descriptor.Uuid.ToString() == UUID.FromString("00002902-0000-1000-8000-00805f9b34fb").ToString())
            {
                if (value.SequenceEqual(BluetoothGattDescriptor.EnableNotificationValue))
                {
                    if (!Worker.SubscribedDevices.ContainsKey(device.Address))
                    {
                        Worker.SubscribedDevices.Add(device.Address, device);
                    }

                }
                else if (value.SequenceEqual(BluetoothGattDescriptor.EnableNotificationValue))
                {
                    if (Worker.SubscribedDevices.ContainsKey(device.Address))
                    {
                        Worker.SubscribedDevices.Remove(device.Address);
                    }
                }
                if (responseNeeded)
                {
                    Worker.GattServer.SendResponse(device,
                            requestId,
                            Android.Bluetooth.GattStatus.Success,
                            0,
                            value);
                }
            }
            else
            {
                if (responseNeeded)
                {
                    Worker.GattServer.SendResponse(device,
                            requestId,
                            Android.Bluetooth.GattStatus.Success,
                            0,
                            null);
                }
            }
        }
    }
}
