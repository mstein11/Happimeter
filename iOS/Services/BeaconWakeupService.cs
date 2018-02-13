using System;
using CoreLocation;
using Foundation;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;

namespace Happimeter.iOS.Services
{
    public class BeaconWakeupService : IBeaconWakeupService
    {
        private CLBeaconRegion BeaconRegion;
        private CLLocationManager LocationManager;
        public BeaconWakeupService()
        {
        }

        private bool AlreadyFired = false;

        public void StartWakeupForBeacon() {
            string message = "";
            CLProximity previousProximity = CLProximity.Far;
            var userId = ServiceLocator.Instance.Get<IAccountStoreService>().GetAccountUserId();
            (var major, var minor) = UtilHelper.GetMajorMinorFromUserId(userId);
            var beaconUuid = UuidHelper.BeaconUuidString;
            BeaconRegion = new CLBeaconRegion(new NSUuid(beaconUuid), (ushort)major, (ushort)minor, "com.example.company");
            BeaconRegion.NotifyEntryStateOnDisplay = true;
            BeaconRegion.NotifyOnExit = true;
            BeaconRegion.NotifyOnEntry = true;

            LocationManager = new CLLocationManager();
            LocationManager.RequestAlwaysAuthorization();

            LocationManager.RegionEntered += (object sender, CLRegionEventArgs e) =>
            {
                Console.WriteLine("RegionEnteredFired hi");
                AlreadyFired = false;

            };

            LocationManager.RegionLeft += (object sender, CLRegionEventArgs e) =>
            {
                Console.WriteLine("RegionLeftFired Bye");
                var btService = ServiceLocator.Instance.Get<IBluetoothService>();
                btService.ExchangeData();
            };



            LocationManager.DidDetermineState += (object sender, CLRegionStateDeterminedEventArgs e) => {

                switch (e.State)
                {
                    case CLRegionState.Inside:
                        Console.WriteLine("region state inside");
                        break;
                    case CLRegionState.Outside:
                        Console.WriteLine("region state outside");
                        break;
                    case CLRegionState.Unknown:
                        Console.WriteLine("region unknown");
                        break;
                    default:
                        Console.WriteLine("region state unknown");
                        break;
                }
            };
            LocationManager.DidStartMonitoringForRegion += (object sender, CLRegionEventArgs e) => {
                LocationManager.RequestState(e.Region);
                string t_region = e.Region.Identifier.ToString();
                Console.WriteLine(t_region);
            };
            LocationManager.DidRangeBeacons += (object sender, CLRegionBeaconsRangedEventArgs e) => {
                if (e.Beacons.Length > 0)
                {
                    CLBeacon beacon = e.Beacons[0];
                    switch (beacon.Proximity)
                    {
                        case CLProximity.Immediate:
                            message = "Immediate";
                            break;
                        case CLProximity.Near:
                            message = "Near";
                            break;
                        case CLProximity.Far:
                            message = "Far";
                            break;
                        case CLProximity.Unknown:
                            message = "Unknown";
                            //SendNotication();
                            break;
                    }

                    if (previousProximity != beacon.Proximity)
                    {
                        Console.WriteLine(message);
                    }
                    previousProximity = beacon.Proximity;
                }
                else
                {
                    Console.WriteLine("nothing");
                }
            };
            LocationManager.StartMonitoring(BeaconRegion);
            LocationManager.MonitoringFailed += (sender, e) => {
                System.Diagnostics.Debug.WriteLine("Failed monitoring");
            };
            LocationManager.StartRangingBeacons(BeaconRegion);
        }
    }
}
