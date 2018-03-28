using System;
using System.Threading;
using System.Threading.Tasks;
using AltBeaconOrg.BoundBeacon;
using Android.App;
using Android.Bluetooth;
using Android.Widget;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.Database;

namespace Happimeter.Watch.Droid.Workers
{
    public class BeaconWorker : AbstractWorker
    {
        private BeaconWorker()
        {
        }
        private CancellationTokenSource TokenSource { get; set; }
        private BeaconTransmitter BeaconTransmitter { get; set; }

        private static BeaconWorker Instance { get; set; }

        public static BeaconWorker GetInstance()
        {
            if (Instance == null)
            {
                Instance = new BeaconWorker();
            }

            return Instance;
        }

        public override void Start()
        {
            if (IsRunning)
            {
                Stop();
            }
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
            BeaconTransmitter = new BeaconTransmitter(Application.Context, beaconParser);

            TokenSource = new CancellationTokenSource();
            if (!BluetoothAdapter.DefaultAdapter.IsEnabled)
            {
                Toast.MakeText(Application.Context, "Bluetooth is not activated", ToastLength.Long).Show();
                return;
            }
            Task.Factory.StartNew(async () =>
            {
                //                IsRunning = true;
                while (IsRunning)
                {
                    //BluetoothAdapter.DefaultAdapter.SetName("Happimeter");
                    BeaconTransmitter.StartAdvertising(beacon, new CallbackAd());
                    System.Diagnostics.Debug.WriteLine("Started Beacon");

                    await Task.Delay(TimeSpan.FromMinutes(5));
                    BeaconTransmitter.StopAdvertising();
                    await Task.Delay(TimeSpan.FromMinutes(5));

                    System.Diagnostics.Debug.WriteLine("Stopped Beacon");
                }
                BeaconTransmitter.Dispose();
                Console.WriteLine($"Stopen Worker: {nameof(BeaconWorker)} because is running became false.");
            }, TokenSource.Token);
        }

        public override void Stop()
        {
            TokenSource.Cancel(false);
            BeaconTransmitter.StopAdvertising();
            BeaconTransmitter.Dispose();
            IsRunning = false;
            Console.WriteLine($"Stopen Worker: {nameof(BeaconWorker)} in Stop method.");
        }
    }
}
