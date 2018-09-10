using System;
using System.Threading;
using System.Threading.Tasks;
using AltBeaconOrg.BoundBeacon;
using Android.App;
using Android.Bluetooth;
using Android.Widget;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using System.Linq;
using Happimeter.Core.Services;

namespace Happimeter.Watch.Droid.Workers
{
    public class BeaconWorker : AbstractWorker
    {
        private BeaconWorker()
        {
        }
        private CancellationTokenSource TokenSource { get; set; }
        private BeaconTransmitter BeaconTransmitter { get; set; }
        private BeaconTransmitter ProximityBeaconTransmitter { get; set; }
        private BeaconType? RunningInType { get; set; }

        private static BeaconWorker Instance { get; set; }

        public static BeaconWorker GetInstance()
        {
            if (Instance == null)
            {
                Instance = new BeaconWorker();
            }

            return Instance;
        }

        public void StartContinously()
        {
            (BeaconTransmitter beaconTransmitter, Beacon beacon) = GetBeaconTransmitterAndBeacon();
            BeaconTransmitter = beaconTransmitter;

            Task.Factory.StartNew(async () =>
            {
                while (IsRunning)
                {
                    //BluetoothAdapter.DefaultAdapter.SetName("Happimeter");
                    BeaconTransmitter.StartAdvertising(beacon, new CallbackAd());
                    System.Diagnostics.Debug.WriteLine("Started Beacon");

                    await Task.Delay(TimeSpan.FromMinutes(BluetoothHelper.BeaconPeriodInSeconds));
                    BeaconTransmitter.StopAdvertising();
                    //from now on we run continously
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    System.Diagnostics.Debug.WriteLine("Stopped Beacon");
                }
                BeaconTransmitter.Dispose();
                Console.WriteLine($"Stopen Worker: {nameof(BeaconWorker)} because is running became false.");
            }, TokenSource.Token);
        }

        public void StartOnce()
        {
            (BeaconTransmitter beaconTransmitter, Beacon beacon) = GetBeaconTransmitterAndBeacon();
            BeaconTransmitter = beaconTransmitter;

            Task.Factory.StartNew(() =>
            {
                IsRunning = true;
                BeaconTransmitter.StartAdvertising(beacon, new CallbackAd());
                System.Diagnostics.Debug.WriteLine("Started Beacon once");
            }, TokenSource.Token);
        }

        private (BeaconTransmitter, Beacon) GetBeaconTransmitterAndBeacon()
        {
            var userId = ServiceLocator.Instance.Get<IDatabaseContext>().Get<BluetoothPairing>(x => x.IsPairingActive)?.PairedWithUserId ?? 0;
            (var major, var minor) = UtilHelper.GetMajorMinorFromUserId(userId);
            string beaconUuid;
            var connectedDevice = BluetoothWorker.GetInstance().SubscribedDevices.Any();
            if (connectedDevice)
            {
                beaconUuid = UuidHelper.BeaconUuidString;
                RunningInType = BeaconType.NormalMode;
            }
            else
            {
                beaconUuid = UuidHelper.WakeupBeaconUuidString;
                RunningInType = BeaconType.WakeupMode;
            }

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
            return (BeaconTransmitter, beacon);
        }

        public void Start()
        {
            if (IsRunning)
            {
                Stop();
            }
            var user = ServiceLocator.Instance.Get<IDatabaseContext>().Get<BluetoothPairing>(x => x.IsPairingActive == true);
            if (!BluetoothAdapter.DefaultAdapter.IsEnabled)
            {
                try
                {
                    Toast.MakeText(Application.Context, "Bluetooth is not activated", ToastLength.Long).Show();
                }
                catch (Exception e)
                {
                    ServiceLocator.Instance.Get<ILoggingService>().LogException(e);
                }
                //we can not start Beacon without BT
                return;
            }
            if (user == null)
            {
                Console.WriteLine("We are not paired, so we should not start beacon");
                //we don't want to start the beacon if not paired
                return;
            }
            TokenSource = new CancellationTokenSource();
            var deviceService = ServiceLocator.Instance.Get<IDeviceService>();
            var isContinous = deviceService.IsContinousMeasurementMode();
            IsRunning = true;
            if (isContinous)
            {
                StartContinously();
            }
            else
            {
                StartOnce();
            }
        }

        public void Stop()
        {
            TokenSource?.Cancel(false);
            if (IsRunning)
            {
                try
                {
                    BeaconTransmitter?.StopAdvertising();
                    BeaconTransmitter?.Dispose();
                }
                catch (System.ArgumentException)
                {
                    //catch handle must be valid exception
                }
                catch (ObjectDisposedException)
                {
                    //catch object disposed exception
                }
            }

            IsRunning = false;
            Console.WriteLine($"Stopen Worker: {nameof(BeaconWorker)} in Stop method.");
        }

        public void EnsureRightBeacontypeIsRunning()
        {
            //todo: only restart if needed
            if (RunningInType != null
                && RunningInType == BeaconType.WakeupMode
                && BluetoothWorker.GetInstance().IsConnected)
            {
                Stop();
                Start();
            }
            else if (RunningInType != null
                && RunningInType == BeaconType.NormalMode
                     && !BluetoothWorker.GetInstance().IsConnected)
            {
                Stop();
                Start();
            }

        }
    }

    public enum BeaconType
    {
        NormalMode,
        WakeupMode
    }
}
