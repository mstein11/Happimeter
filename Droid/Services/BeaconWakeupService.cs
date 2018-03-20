using System;
using Happimeter.Interfaces;

using AltBeaconOrg.BoundBeacon;
using Happimeter.Core.Helper;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using Plugin.CurrentActivity;
using Happimeter.Services;

namespace Happimeter.Droid.Services
{
    public class BeaconWakeupService : Java.Lang.Object, IBeaconWakeupService, IBeaconConsumer
    {
        private BeaconManager BeaconManager { get; set; }
        private readonly MonitorNotifier _monitorNotifier = new MonitorNotifier();

        public BeaconWakeupService()
        {
        }

        public Context ApplicationContext => CrossCurrentActivity.Current.Activity;

        public bool BindService(Intent intent, IServiceConnection serviceConnection, [GeneratedEnum] Bind flags)
        {
            return ApplicationContext.BindService(intent, serviceConnection, flags);
        }

        public void OnBeaconServiceConnect()
        {
            BeaconManager.SetForegroundBetweenScanPeriod(TimeSpan.FromMinutes(5).Seconds * 1000);
            BeaconManager.SetBackgroundScanPeriod(TimeSpan.FromMinutes(5).Seconds * 1000);
            BeaconManager.SetMonitorNotifier(_monitorNotifier);
            _monitorNotifier.EnterRegionComplete += (sender, e) => {
                var btService = ServiceLocator.Instance.Get<IBluetoothService>();
                ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.BeaconRegionEnteredEvent);
                btService.ExchangeData();
            };

            _monitorNotifier.ExitRegionComplete += (sender, e) => {
                ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.BeaconRegionLeftEvent);
            };

            var userId = ServiceLocator.Instance.Get<IAccountStoreService>().GetAccountUserId();
            var tupple = UtilHelper.GetMajorMinorFromUserId(userId);
            //a region constitutes the beacon that should be found. 
            var region = new AltBeaconOrg.BoundBeacon.Region("com.example.company", Identifier.Parse(UuidHelper.BeaconUuidString), Identifier.FromInt(tupple.Item1), Identifier.FromInt(tupple.Item2));

            BeaconManager.StartMonitoringBeaconsInRegion(region);
        }

        public void UnbindService(IServiceConnection serviceConnection)
        {
            ApplicationContext.UnbindService(serviceConnection);
        }

        public void StartWakeupForBeacon()
        {

            BeaconManager = BeaconManager.GetInstanceForApplication(CrossCurrentActivity.Current.Activity);
            var iBeaconParser = new BeaconParser();
            //  Estimote > 2013
            iBeaconParser.SetBeaconLayout(UuidHelper.BeaconLayout);
            BeaconManager.BeaconParsers.Add(iBeaconParser);
            BeaconManager.Bind(this);
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

    public class MonitorEventArgs : EventArgs
    {
        public Region Region { get; set; }
        public int State { get; set; }
    }

    public class MonitorNotifier : Java.Lang.Object, IMonitorNotifier
    {
        public event EventHandler<MonitorEventArgs> DetermineStateForRegionComplete;
        public event EventHandler<MonitorEventArgs> EnterRegionComplete;
        public event EventHandler<MonitorEventArgs> ExitRegionComplete;

        public void DidDetermineStateForRegion(int state, Region region)
        {
            OnDetermineStateForRegionComplete(state, region);
        }

        public void DidEnterRegion(Region region)
        {
            OnEnterRegionComplete(region);
        }

        public void DidExitRegion(Region region)
        {
            OnExitRegionComplete(region);
        }

        private void OnDetermineStateForRegionComplete(int state, Region region)
        {
            if (DetermineStateForRegionComplete != null)
            {
                DetermineStateForRegionComplete(this, new MonitorEventArgs { State = state, Region = region });
            }
        }

        private void OnEnterRegionComplete(Region region)
        {
            if (EnterRegionComplete != null)
            {
                EnterRegionComplete(this, new MonitorEventArgs { Region = region });
            }
        }

        private void OnExitRegionComplete(Region region)
        {
            if (ExitRegionComplete != null)
            {
                ExitRegionComplete(this, new MonitorEventArgs { Region = region });
            }
        }
    }
}
