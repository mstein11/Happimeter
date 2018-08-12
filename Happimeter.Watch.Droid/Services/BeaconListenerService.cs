using System;

using AltBeaconOrg.BoundBeacon;
using Happimeter.Core.Helper;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;

using Android.OS;
using AltBeaconOrg.BoundBeacon.Powersave;
using Android.App;
using System.Collections.Concurrent;
using System.Linq;
using Plugin.BluetoothLE;
using Happimeter.Watch.Droid.ServicesBusinessLogic;

namespace Happimeter.Watch.Droid.Services
{
    public class BeaconListenerService : Java.Lang.Object, IBeaconConsumer
    {
        private BeaconManager BeaconManager { get; set; }
        private List<Region> ToMonitor { get; set; } = new List<Region>();
        private readonly RangeNotifier _rangeNotifier = new RangeNotifier();
        private BackgroundPowerSaver PowerSave { get; set; }

        public static ConcurrentBag<(int, double)> ProximityMeasures = new ConcurrentBag<(int, double)>();

        public BeaconListenerService()
        {
        }

        public Context ApplicationContext => Application.Context;

        public bool BindService(Intent intent, IServiceConnection serviceConnection, [GeneratedEnum] Bind flags)
        {
            return ApplicationContext.BindService(intent, serviceConnection, flags);
        }

        public void OnBeaconServiceConnect()
        {
            var measurementMode = ServiceLocator.Instance.Get<IDeviceService>().GetMeasurementMode();
            var runfor = 60;
            var breakFor = 60;
            if (measurementMode.IntervalSeconds != null && measurementMode.FactorMeasurementOfInterval != null)
            {
                breakFor = measurementMode.IntervalSeconds.Value - (measurementMode.IntervalSeconds.Value / measurementMode.FactorMeasurementOfInterval.Value);
                runfor = measurementMode.IntervalSeconds.Value / measurementMode.FactorMeasurementOfInterval.Value;
            }


            BeaconManager.ForegroundScanPeriod = runfor * 1000;
            BeaconManager.BackgroundScanPeriod = runfor * 1000;
            BeaconManager.BackgroundBetweenScanPeriod = breakFor * 1000;
            BeaconManager.ForegroundBetweenScanPeriod = breakFor * 1000;

            PowerSave = new BackgroundPowerSaver(ApplicationContext);

            _rangeNotifier.DidRangeBeaconsInRegionComplete += _rangeNotifier_DidRangeBeaconsInRegionComplete;

            var region = new AltBeaconOrg.BoundBeacon.Region("com.company.name", Identifier.Parse(UuidHelper.BeaconUuidString), null, null);
            var region1 = new AltBeaconOrg.BoundBeacon.Region("com.company.name1", Identifier.Parse(UuidHelper.WakeupBeaconUuidString), null, null);
            ToMonitor.Add(region);
            ToMonitor.Add(region1);

            BeaconManager.AddRangeNotifier(_rangeNotifier);
            BeaconManager.StartRangingBeaconsInRegion(region);
            BeaconManager.StartRangingBeaconsInRegion(region1);
        }

        public void UnbindService(IServiceConnection serviceConnection)
        {
            ApplicationContext.UnbindService(serviceConnection);
        }

        public void StartListeningForBeacons()
        {
#if DEBUG
            //BeaconManager.SetDebug(true);
#endif
            BeaconManager = BeaconManager.GetInstanceForApplication(Application.Context);
            var iBeaconParser = new BeaconParser();
            //  ibeacon layout
            iBeaconParser.SetBeaconLayout(UuidHelper.BeaconLayout);
            BeaconManager.BeaconParsers.Add(iBeaconParser);
            BeaconManager.Bind(this);
        }

        public void StopListeningForBeacons()
        {
            if (BeaconManager != null && ToMonitor != null && ToMonitor.Any())
            {
                foreach (var item in ToMonitor)
                {
                    BeaconManager.StopRangingBeaconsInRegion(item);
                }

            }
            if (BeaconManager != null)
            {
                BeaconManager.Unbind(this);
            }

        }


        void _rangeNotifier_DidRangeBeaconsInRegionComplete(object sender, RangeEventArgs e)
        {
            try
            {
                Console.WriteLine("Did Range yo");
                foreach (var beacon in e.Beacons)
                {
                    var userId = UtilHelper.GetUserIdFromMajorMinor(beacon.Id2.ToInt(), beacon.Id3.ToInt());
                    Console.WriteLine($"userId {userId}; Distance {beacon.Distance}");
                    ProximityMeasures.Add((userId, beacon.Distance));
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Caught error in ranging");
            }
        }

    }

    public class RangeEventArgs : EventArgs
    {
        public Region Region { get; set; }
        public ICollection<Beacon> Beacons { get; set; }
    }

    public class RangeNotifier : Java.Lang.Object, IRangeNotifier
    {
        public event EventHandler<RangeEventArgs> DidRangeBeaconsInRegionComplete;

        public void DidRangeBeaconsInRegion(ICollection<Beacon> beacons, Region region)
        {
            OnDidRangeBeaconsInRegion(beacons, region);
        }

        void OnDidRangeBeaconsInRegion(ICollection<Beacon> beacons, Region region)
        {
            if (DidRangeBeaconsInRegionComplete != null)
            {
                DidRangeBeaconsInRegionComplete(this, new RangeEventArgs { Beacons = beacons, Region = region });
            }
        }
    }
}
