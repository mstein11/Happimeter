using System;
using AltBeaconOrg.BoundBeacon;
using AltBeaconOrg.BoundBeacon.Startup;
using Android.App;
using Java.Lang;
using Plugin.BluetoothLE;
using Android.Speech.Tts;
using AltBeaconOrg.Bluetooth;
namespace Happimeter.Watch.Droid
{
    public class MainApplication : Application, IBootstrapNotifier
    {
        //private BackgroundPowerSaver _backgroundPowerSaver;
        private bool _haveDetectedBeaconsSinceBoot = false;
        //private MonitoringActivity monitoringActivity = null;

        private BeaconManager BeaconManager { get; set; }
        private RegionBootstrap regionBootstrap;

        public MainApplication()
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            BluetoothMedic.Instance.EnablePowerCycleOnFailures(this);
            BluetoothMedic.Instance.SetNotificationsEnabled(true, Resource.Drawable.notification_icon_background);
            // To detect proprietary beacons, you must add a line like below corresponding to your beacon
            // type.  Do a web search for "setBeaconLayout" to get the proper expression.
            // beaconManager.getBeaconParsers().add(new BeaconParser().
            //        setBeaconLayout("m:2-3=beac,i:4-19,i:20-21,i:22-23,p:24-24,d:25-25"));
            /*
			BeaconManager = BeaconManager.GetInstanceForApplication(this);
			var iBeaconParser = new BeaconParser();
			//  Estimote > 2013
			iBeaconParser.SetBeaconLayout(UuidHelper.BeaconLayout);
			BeaconManager.BeaconParsers.Add(iBeaconParser);
			BeaconManager.Bind(this);
          


			// wake up the app when any beacon is seen (you can specify specific id filers in the parameters below)
			Region region = new Region("com.example.myapp.boostrapRegion", null, null, null);

			RegionBootstrap = new RegionBootstrap(this, region);
			*/
        }

        public void DidDetermineStateForRegion(int state, Region region)
        {
            //throw new NotImplementedException();
        }

        public void DidEnterRegion(Region region)
        {
            //throw new NotImplementedException();
        }

        public void DidExitRegion(Region region)
        {
            //throw new NotImplementedException();
        }
    }
}
