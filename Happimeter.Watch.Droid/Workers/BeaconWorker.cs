using System;
using System.Threading.Tasks;
using AltBeaconOrg.BoundBeacon;
using Android.App;
using Android.Bluetooth;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.Database;

namespace Happimeter.Watch.Droid.Workers
{
    public class BeaconWorker : AbstractWorker
    {
        private BeaconWorker()
        {
        }

        private static BeaconWorker Instance { get; set; }

        public static BeaconWorker GetInstance()
        {
            if (Instance == null)
            {
                Instance = new BeaconWorker();
            }

            return Instance;
        }

        public override async void Start()
        {
            var userId = ServiceLocator.Instance.Get<IDatabaseContext>().Get<BluetoothPairing>(x => x.IsPairingActive)?.PairedWithUserId ?? 0;
            (var major, var minor) = UtilHelper.GetMajorMinorFromUserId(userId);
            var beaconUuid = UuidHelper.BeaconUuidString;
            var beacon = new Beacon.Builder()
                                   .SetId1(beaconUuid)
                                   .SetId2(major.ToString())
                                   .SetId3(minor.ToString())
                                   .SetManufacturer(UuidHelper.BeaconManufacturerId) // Radius Networks.0x0118  Change this for other beacon layouts//0x004C for iPhone
                                   .SetTxPower(UuidHelper.TxPowerLevel) // Power in dB
                                   //.SetBluetoothName("Happimeter")
                                   .Build();
            var beaconParser = new BeaconParser().SetBeaconLayout(UuidHelper.BeaconLayout);
            var trans = new BeaconTransmitter(Application.Context, beaconParser);
            IsRunning = true;

            while (IsRunning)
            {
                //BluetoothAdapter.DefaultAdapter.SetName("Happimeter");
                trans.StartAdvertising(beacon, new CallbackAd());
                System.Diagnostics.Debug.WriteLine("Started Beacon");

                await Task.Delay(TimeSpan.FromMinutes(1));
                trans.StopAdvertising();
                await Task.Delay(TimeSpan.FromMinutes(1));

                System.Diagnostics.Debug.WriteLine("Stopped Beacon");

            }
        }

        public override void Stop()
        {
            IsRunning = false;
        }
    }
}
