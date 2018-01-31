using System;
using CoreLocation;
using Foundation;
using UIKit;

namespace Happimeter.iOS
{
    public partial class AboutViewController : UIViewController
    {
        public AboutViewModel ViewModel { get; set; }
        public AboutViewController(IntPtr handle) : base(handle)
        {
            ViewModel = new AboutViewModel();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = ViewModel.Title;

            AppNameLabel.Text = "Happimeter";
            VersionLabel.Text = "1.0";
            AboutTextView.Text = "This app is written in C# and native APIs using the Xamarin Platform. It shares code with its iOS, Android, & Windows versions.";


            string message = "";
            CLProximity previousProximity = CLProximity.Far;
            var beaconRegion = new CLBeaconRegion(new NSUuid("2f234454-cf6d-4a0f-adf2-f4911ba9ffa6"), "com.example.company");
            beaconRegion.NotifyEntryStateOnDisplay = true;
            beaconRegion.NotifyOnExit = true;
            beaconRegion.NotifyOnEntry = true;
            var locationMgr = new CLLocationManager();
            locationMgr.RequestAlwaysAuthorization();

            locationMgr.RegionEntered += (object sender, CLRegionEventArgs e) =>
            {
                Console.WriteLine("Hello");
            };

            locationMgr.RegionLeft += (object sender, CLRegionEventArgs e) =>
            {
                Console.WriteLine("Bye");
            };



            locationMgr.DidDetermineState += (object sender, CLRegionStateDeterminedEventArgs e) => {

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
            locationMgr.DidStartMonitoringForRegion += (object sender, CLRegionEventArgs e) => {
                locationMgr.RequestState(e.Region);
                string t_region = e.Region.Identifier.ToString();
                Console.WriteLine(t_region);
            };
            locationMgr.DidRangeBeacons += (object sender, CLRegionBeaconsRangedEventArgs e) => {
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
            locationMgr.StartMonitoring(beaconRegion);
            locationMgr.StartRangingBeacons(beaconRegion);

        }

        partial void ReadMoreButton_TouchUpInside(UIButton sender) => ViewModel.OpenWebCommand.Execute(null);
    }
}
