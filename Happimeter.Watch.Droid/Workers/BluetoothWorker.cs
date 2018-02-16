using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AltBeaconOrg.BoundBeacon;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.OS;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.Bluetooth;
using Happimeter.Watch.Droid.Database;
using Java.Util;

namespace Happimeter.Watch.Droid.Workers
{
    public class BluetoothWorker : AbstractWorker
    {
        public BluetoothManager Manager;
        public BluetoothGattServer GattServer;
        public Dictionary<string, BluetoothGatt> GattClients = new Dictionary<string, BluetoothGatt>();

        public Dictionary<string, BluetoothDevice> SubscribedDevices = new Dictionary<string, BluetoothDevice>();
        public Dictionary<string, int> DevicesMtu = new Dictionary<string, int>();

        private CancellationTokenSource TokenSource { get; set; }

        public BluetoothWorker()
        {
            Manager = (BluetoothManager)Application.Context.GetSystemService(Android.Content.Context.BluetoothService);
        }

        public override void Start()
        {
            TokenSource = new CancellationTokenSource();
            if (!BluetoothAdapter.DefaultAdapter.IsEnabled)
            {
                return;
            }
            Task.Factory.StartNew(() => {
                IsRunning = true;
                InitializeGatt();
                ConnectableAdvertisement();
            }, TokenSource.Token);

            /*System.Diagnostics.Debug.WriteLine("Started Bluetooth Worker");
            var pairing = ServiceLocator.Instance.Get<IDatabaseContext>().GetCurrentBluetoothPairing();
            if (pairing == null) {
                System.Diagnostics.Debug.WriteLine("No pairing found, we start in Auth mode");
                //RunAuthAdvertiser();
                ConnectableAdvertisement();
            } else {
                System.Diagnostics.Debug.WriteLine("Found pairing, we start in beacon mode");
                ConnectableAdvertisement();
            }
            */
        }

        private void InitializeGatt() {
            GattServer = Manager.OpenGattServer(Application.Context, new CallbackGatt(this));
            GattServer.AddService(HappimeterService.Create());
            System.Diagnostics.Debug.WriteLine("Gatt initialized");
        }

        public override void Stop()
        {
            TokenSource.Cancel(false);
            if (GattServer != null) {
                GattServer.Close();
                GattServer.Dispose();                
            }
            Manager.Adapter.BluetoothLeAdvertiser?.StopAdvertising(new CallbackAd());
            Manager.Adapter.BluetoothLeAdvertiser?.Dispose();

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

            /*
            (var major, var minor) = UtilHelper.GetMajorMinorFromUserId(userId);
            var beaconUuid = UuidHelper.BeaconUuidString;
            var beacon = new Beacon.Builder()
                                   .SetId1(beaconUuid)
                                   .SetId2(major.ToString())
                                   .SetId3(minor.ToString())
                                   .SetManufacturer(UuidHelper.BeaconManufacturerId) // Radius Networks.0x0118  Change this for other beacon layouts//0x004C for iPhone
                                   .SetTxPower(UuidHelper.TxPowerLevel) // Power in dB
                                                             //.SetBluetoothName("Happimeter AAAA")
                                   .Build();
            var beaconParser = new BeaconParser().SetBeaconLayout(UuidHelper.BeaconLayout);
            var trans = new BeaconTransmitter(Application.Context, beaconParser);

            while(IsRunning && false) {
                System.Diagnostics.Debug.WriteLine("About to start beacon");
                trans.StartAdvertising(beacon, new CallbackAd());
                System.Diagnostics.Debug.WriteLine("Started Beacon");
                await Task.Delay(TimeSpan.FromMinutes(1));
                trans.StopAdvertising();
                await Task.Delay(TimeSpan.FromMinutes(1));
                System.Diagnostics.Debug.WriteLine("Stopped Beacon");

            }
            */
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

            var userId = ServiceLocator.Instance.Get<IDatabaseContext>().Get<BluetoothPairing>(x => x.IsPairingActive)?.PairedWithUserId ?? 0;

            var tmpr = BluetoothAdapter.DefaultAdapter.SetName("Happimeter AAAA");
            var data = new AdvertiseData.Builder()
                                        .SetIncludeDeviceName(true)
                                        //.SetIncludeTxPowerLevel(true)
                                        //.AddServiceUuid(ParcelUuid.FromString(BeaconUuid))
                                        //todo: add appropriate serviceId
                                        .AddServiceUuid(ParcelUuid.FromString("0000F0F0-0000-1000-8000-00805F9B34FB"))
                                        .AddServiceData(ParcelUuid.FromString("0000F0F0-0000-1000-8000-00805F9B34FB"), Encoding.UTF8.GetBytes(userId.ToString()))
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
            var userId = ServiceLocator.Instance.Get<IDatabaseContext>().Get<BluetoothPairing>(x => x.IsPairingActive)?.PairedWithUserId ?? 0;
            var data = new AdvertiseData.Builder()
                                        .SetIncludeDeviceName(true)
                                        //.SetIncludeTxPowerLevel(true)
                                        //.AddServiceUuid(ParcelUuid.FromString(BeaconUuid))
                                        //todo: add appropriate serviceId
                                        .AddServiceUuid(ParcelUuid.FromString("0000F0F0-0000-1000-8000-00805F9B34FB"))
                                        .AddServiceData(ParcelUuid.FromString("0000F0F0-0000-1000-8000-00805F9B34FB"), Encoding.UTF8.GetBytes(userId.ToString()))
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

        public override void OnConnectionStateChange(BluetoothDevice device, ProfileState status, ProfileState newState)
        {
            base.OnConnectionStateChange(device, status, newState);

            if (newState == ProfileState.Connected) {
                Console.WriteLine("device is now connected: watch as server");
                var client = device.ConnectGatt(Application.Context, true, new CallBackGattClient(Worker));
                if (!Worker.GattClients.ContainsKey(device.Address)) {
                    Worker.GattClients.Add(device.Address, client);
                }
            }
        }

        public override void OnCharacteristicReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattCharacteristic characteristic)
        {
            base.OnCharacteristicReadRequest(device, requestId, offset, characteristic);

            var authCharac = characteristic as HappimeterAuthCharacteristic;
            if (authCharac != null) {
                authCharac.HandleRead(device, requestId, offset, Worker);
                return;
            }


            var dataCharac = characteristic as HappimeterDataCharacteristic;
            if (dataCharac != null)
            {
                dataCharac.HandleRead(device, requestId, offset, Worker);
                return;
            }

            Worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, Encoding.UTF8.GetBytes("Unknwon characteristic!"));
        }

        public override void OnCharacteristicWriteRequest(BluetoothDevice device, int requestId, BluetoothGattCharacteristic characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
        {
            base.OnCharacteristicWriteRequest(device, requestId, characteristic, preparedWrite, responseNeeded, offset, value);

            var authCharac = characteristic as HappimeterAuthCharacteristic;
            if (authCharac != null)
            {
                authCharac.HandleWrite(device, requestId, preparedWrite, responseNeeded, offset, value, Worker);
                return;
            }

            var dataCharac = characteristic as HappimeterDataCharacteristic;
            if (dataCharac != null)
            {
                dataCharac.HandleWriteAsync(device, requestId, preparedWrite, responseNeeded, offset, value, Worker);
                return;
            }

            Worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, Encoding.UTF8.GetBytes("Unknwon characteristic!"));
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
                                               Android.Bluetooth.GattStatus.Success,
                    0,
                    valueToReturn.ToArray());
            }

            Worker.GattServer.SendResponse(device,
                            requestId,
                            Android.Bluetooth.GattStatus.Success,
                            0,
                            null);
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

        public override void OnMtuChanged(BluetoothDevice device, int mtu)
        {
            base.OnMtuChanged(device, mtu);

            if (!Worker.DevicesMtu.ContainsKey(device.Address))
            {
                Worker.DevicesMtu.Add(device.Address, mtu);
            } else {
                Worker.DevicesMtu[device.Address] = mtu;
            }
        }
    }

    public class CallBackGattClient : BluetoothGattCallback
    {

        public BluetoothWorker Worker { get; set; }

        public CallBackGattClient(BluetoothWorker worker)
        {
            Worker = worker;
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, Android.Bluetooth.GattStatus status, ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);
            Console.WriteLine("device is now connected: watch as client");
            if (newState == ProfileState.Connected) {
                var success = gatt.RequestMtu(512);
            }
        }

        public override void OnMtuChanged(BluetoothGatt gatt, int mtu, Android.Bluetooth.GattStatus status)
        {
            base.OnMtuChanged(gatt, mtu, status);

            if (!Worker.DevicesMtu.ContainsKey(gatt.Device.Address))
            {
                Worker.DevicesMtu.Add(gatt.Device.Address, mtu);
            }
            else
            {
                Worker.DevicesMtu[gatt.Device.Address] = mtu;
            }
        }
    }
}
