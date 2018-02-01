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
using Java.Util;
using Plugin.BluetoothLE;
using Plugin.BluetoothLE.Server;

namespace Happimeter.Watch.Droid.Workers
{
    public class BluetoothWorker : AbstractWorker
    {
        //private const string ServiceGuid = "62b2c2a5-4bc3-4621-86ea-abbe4673be42";
        //private const string ReadWriteCharacteristic = "7918ec07-2ba4-4542-aa13-0a10ff3826ba";
        //private const string NotifyCharacteristic = "1b4dc745-1929-485a-93f6-a76109f02bd6";

        //also service uuid for all Gatt things
        //private const string BeaconUuid = "00000001-0000-1000-1000-00911BA9FFA6";
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

        public BluetoothWorker()
        {
            Manager = (BluetoothManager)Application.Context.GetSystemService(Android.Content.Context.BluetoothService);
			GattServer = Manager.OpenGattServer(Application.Context, new CallbackGatt(this));
        }

        public override void Start()
        {
            IsRunning = true;
            RunAuthAdvertiser();
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

        public async Task RunBeacon() {
            var uuid = Encoding.UTF8.GetBytes(BeaconUuid);

            var beacon = new Beacon.Builder()
                                   .SetId1(BeaconUuid)
                                   .SetId2(Major)
                                   .SetId3(Minor)
                                   .SetManufacturer(ManufacturerCode) // Radius Networks.0x0118  Change this for other beacon layouts//0x004C for iPhone
                                   .SetTxPower(TxPowerLevel) // Power in dB
                                   .Build();

            var beaconParser = new BeaconParser().SetBeaconLayout(BeaconLayout);
            var trans = new BeaconTransmitter(Application.Context, beaconParser);
            trans.StartAdvertising(beacon, new CallbackAd());
        }

        public void AdvertiseExistance() {
            
        }

        private async Task RunAuthAdvertiser()
        {

            //var service = new BluetoothGattService(UUID.FromString("00000000-0000-1000-8000-00805F9B34FB"), GattServiceType.Primary);
            //var characteristic = new BluetoothGattCharacteristic(UUID.FromString(ReadWriteCharacteristic), GattProperty.Read | GattProperty.Write, GattPermission.Read | GattPermission.Write);

            //service.AddCharacteristic(characteristic);
            GattServer.AddService(HappimeterService.Create());
            BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser.StopAdvertising(new CallbackAd());
            var settings = new AdvertiseSettings.Builder()
                                                .SetAdvertiseMode(AdvertiseMode.LowLatency)
                                                .SetTxPowerLevel(AdvertiseTx.PowerHigh)
                                                .SetConnectable(true)
                                                .Build();

            var tmpr = BluetoothAdapter.DefaultAdapter.SetName("Happimeter AAAA");
            if (tmpr)
            {
                System.Diagnostics.Debug.WriteLine("Set Name");
            }

            var data = new AdvertiseData.Builder()
                                        .SetIncludeDeviceName(true)
                                        .SetIncludeTxPowerLevel(true)
                                        //.AddServiceUuid(ParcelUuid.FromString(BeaconUuid))
                                        //todo: add appropriate serviceId
                                        .AddServiceUuid(ParcelUuid.FromString("F0000000-0000-1000-8000-00805F9B34FB"))
                                        .Build();

            BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser.StartAdvertising(settings, data, new CallbackAd());

            var counter = 0;
        }
    }

    public class CallbackAd : AdvertiseCallback
    {
        public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
        {
            base.OnStartSuccess(settingsInEffect);
        }

        public override void OnStartFailure(AdvertiseFailure errorCode)
        {
            base.OnStartFailure(errorCode);
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

            if (AuthenticationDeviceDidGreat.ContainsKey(device.Address) && AuthenticationDeviceDidGreat[device.Address]) {
                //device did great first, lets tell him our beacon information

                var jsonString = string.Format("{{'UuId':'{0}', 'Minor':{1}, 'Major':{2} }}", BluetoothWorker.BeaconUuid, BluetoothWorker.Minor, BluetoothWorker.Major );
                var bytes = Encoding.UTF8.GetBytes(jsonString);
                Worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, bytes);
                Worker.RunBeacon();
            } else {
                //we don't know this device
                Worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Failure, offset, Encoding.ASCII.GetBytes("H"));    
            }
        }

        public override void OnCharacteristicWriteRequest(BluetoothDevice device, int requestId, BluetoothGattCharacteristic characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
        {
            base.OnCharacteristicWriteRequest(device, requestId, characteristic, preparedWrite, responseNeeded, offset, value);

            if (characteristic.Uuid.ToString() == UUID.FromString(HappimeterAuthCharacteristic.CharacteristicUuid).ToString()) {
                System.Diagnostics.Debug.WriteLine("Write received");
                AuthenticationDeviceDidGreat.Add(device.Address, true);


            }


            Worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, Encoding.UTF8.GetBytes("ab"));
            var devices = Worker.Manager.GetConnectedDevices(ProfileType.Gatt);

            //does not work
            characteristic.SetValue(Encoding.UTF8.GetBytes("Hello!!!"));
            Worker.GattServer.NotifyCharacteristicChanged(device, characteristic, false);
        }

        public override void OnConnectionStateChange(BluetoothDevice device, ProfileState status, ProfileState newState)
        {
            base.OnConnectionStateChange(device, status, newState);
        }

        public override void OnDescriptorReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattDescriptor descriptor)
        {
            base.OnDescriptorReadRequest(device, requestId, offset, descriptor);

            Worker.GattServer.SendResponse(device,requestId, Android.Bluetooth.GattStatus.Success, offset, null);
        }

        public override void OnServiceAdded(Android.Bluetooth.GattStatus status, BluetoothGattService service)
        {
            base.OnServiceAdded(status, service);
        }

        public override void OnDescriptorWriteRequest(BluetoothDevice device, int requestId, BluetoothGattDescriptor descriptor, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
        {
            base.OnDescriptorWriteRequest(device, requestId, descriptor, preparedWrite, responseNeeded, offset, value);

            Worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, value);
        }

        public override void OnMtuChanged(BluetoothDevice device, int mtu)
        {
            base.OnMtuChanged(device, mtu);
        }

        public override void OnExecuteWrite(BluetoothDevice device, int requestId, bool execute)
        {
            base.OnExecuteWrite(device, requestId, execute);
        }

        public override void OnNotificationSent(BluetoothDevice device, Android.Bluetooth.GattStatus status)
        {
            base.OnNotificationSent(device, status);
        }
    }

    /*
        private async System.Threading.Tasks.Task RunAsync()
        {

            try
            {
                var uuId = Guid.Parse(ServiceGuid);
                var server = CrossBleAdapter.Current.CreateGattServer();
                var service = server.CreateService(uuId, true);
                //var service = server.AddService(uuId, true);
                var characteristic = service.AddCharacteristic(
                    Guid.Parse(ReadWriteCharacteristic),
                    CharacteristicProperties.Read | CharacteristicProperties.Write,
                    GattPermissions.Read | GattPermissions.Write
                );

                var notifyCharacteristic = service.AddCharacteristic
                (
                    Guid.Parse(NotifyCharacteristic),
                    CharacteristicProperties.Indicate | CharacteristicProperties.Notify,
                    GattPermissions.Read | GattPermissions.Write
                );

                IDisposable notifyBroadcast = null;
                notifyCharacteristic.WhenDeviceSubscriptionChanged().Subscribe(e =>
                {
                    var @event = e.IsSubscribed ? "Subscribed" : "Unsubcribed";

                    if (notifyBroadcast == null)
                    {
                        notifyBroadcast = System.Reactive.Linq.Observable
                            .Interval(TimeSpan.FromSeconds(1))
                            .Where(x => notifyCharacteristic.SubscribedDevices.Count > 0)
                            .Subscribe(_ =>
                            {
                                System.Diagnostics.Debug.WriteLine("Sending Broadcast");
                                var dt = DateTime.Now.ToString("g");
                                var bytes = Encoding.UTF8.GetBytes(dt);
                                notifyCharacteristic.Broadcast(bytes);
                            });
                    }
                });

                characteristic.WhenReadReceived().Subscribe(x =>
                {
                    var write = "HELLO";

                    // you must set a reply value
                    x.Value = Encoding.UTF8.GetBytes(write);

                    x.Status = Plugin.BluetoothLE.Server.GattStatus.Success; // you can optionally set a status, but it defaults to Success
                });
                characteristic.WhenWriteReceived().Subscribe(x =>
                {
                    var write = Encoding.UTF8.GetString(x.Value, 0, x.Value.Length);
                    // do something value
                });

                var advertisementData = new Plugin.BluetoothLE.Server.AdvertisementData
                {
                    LocalName = "TestServer",
                    //ManufacturerData = new ManufacturerData { CompanyId = 123, Data = Encoding.UTF8.GetBytes("Happimeter") },
                    //ServiceUuids = new System.Collections.Generic.List<Guid> {Guid.Parse(ServiceGuid)}
                };
                server.AddService(service);
                //await server.Start(advertisementData);
                CrossBleAdapter.Current.Advertiser.Start(new Plugin.BluetoothLE.Server.AdvertisementData
                {
                    LocalName = "TestServer",
                    //ServiceUuids = new Guid[] { Guid.Parse(ServiceGuid) }.ToList()
                });

                while (true)
                {
                    await Task.Delay(1000);
                    System.Diagnostics.Debug.WriteLine("Is Running: ", server);
                }


            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
            }
        }
        */

    /*
     * 
     * manufacturerData[0] = 0xBE; //Beacon Identifier
            manufacturerData[1] = 0xAC; //Beacon Identifier



            for (var i = 2; i <= 17; i++) {
                manufacturerData[i] = uuid[i - 2];    
            }
            manufacturerData[18] = 0x00; // first byte of Major
            manufacturerData[19] = 0x09; // second byte of Major
            manufacturerData[20] = 0x00; // first minor
            manufacturerData[21] = 0x06; // second minor
            manufacturerData[22] = 0xB5; // txpower


            var manufacturerData2 = new byte[30];
            manufacturerData2[0] = 0x02;
            manufacturerData2[1] = 0x01;
            manufacturerData2[2] = 0x06;
            manufacturerData2[3] = 0x1a;
            manufacturerData2[4] = 0xff;
            manufacturerData2[5] = 0x4c;
            manufacturerData2[6] = 0x00;
            manufacturerData2[7] = 0x02;
            manufacturerData2[8] = 0x15;

            for (var i = 1; i <= 16; i++) {
                manufacturerData2[8 + i] = (byte)i;
            }

            manufacturerData2[25] = 0x00; // first byte of Major
            manufacturerData2[26] = 0x09; // second byte of Major
            manufacturerData2[27] = 0x00; // first minor
            manufacturerData2[28] = 0x06; // second minor
            manufacturerData2[29] = 0xB5; // txpower
            //manufacturerData2[30] = 0xB5; // txpower

            var settings = new AdvertiseSettings.Builder()
                                                .SetAdvertiseMode(AdvertiseMode.LowPower)
                                                .SetTxPowerLevel(AdvertiseTx.PowerMedium)
                                                .SetTimeout(0)
                                                .SetConnectable(false)
                                                .Build();

            var tmpr = BluetoothAdapter.DefaultAdapter.SetName("Happimeter");
            if (tmpr)
            {
                System.Diagnostics.Debug.WriteLine("Set Name");
            }

            var data = new AdvertiseData.Builder()
                                        //apple
                                        .AddManufacturerData(76, manufacturerData2)
                                        .SetIncludeDeviceName(false)
                                        .SetIncludeTxPowerLevel(false)
                                        //.
                                        //google
                                        //.AddManufacturerData(224, manufacturerData)
                                        .Build();

            BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser.StartAdvertising(settings, data, new CallbackAd());

     */
}
