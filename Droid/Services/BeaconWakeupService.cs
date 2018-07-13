using System;
using Happimeter.Interfaces;

using AltBeaconOrg.BoundBeacon;
using Happimeter.Core.Helper;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using Plugin.CurrentActivity;
using Happimeter.Services;
using Android.OS;
using AltBeaconOrg.BoundBeacon.Powersave;
using Happimeter.Core.Services;

namespace Happimeter.Droid.Services
{
	public class BeaconWakeupService : Java.Lang.Object, IBeaconWakeupService, IBeaconConsumer
	{
		private BeaconManager BeaconManager { get; set; }
		private readonly MonitorNotifier _monitorNotifier = new MonitorNotifier();
		private BackgroundPowerSaver PowerSave { get; set; }
		public BeaconWakeupService()
		{
		}

		public Context ApplicationContext => MainApplication.Context;

		public bool BindService(Intent intent, IServiceConnection serviceConnection, [GeneratedEnum] Bind flags)
		{
			return ApplicationContext.BindService(intent, serviceConnection, flags);
		}

		public void OnBeaconServiceConnect()
		{
			BeaconManager.SetForegroundScanPeriod(60 * 1000);
			BeaconManager.SetBackgroundScanPeriod(60 * 1000);
			BeaconManager.SetBackgroundBetweenScanPeriod(300 * 1000);
			BeaconManager.SetForegroundBetweenScanPeriod(60 * 1000);
			PowerSave = new BackgroundPowerSaver(ApplicationContext);
			_monitorNotifier.EnterRegionComplete += (sender, e) =>
			{
				Console.WriteLine("Did enter region");
				var btService = ServiceLocator.Instance.Get<IBluetoothService>();
				ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.BeaconRegionEnteredEvent);
				btService.Init(force: true);
			};

			_monitorNotifier.ExitRegionComplete += (sender, e) =>
			{
				Console.WriteLine("Did leave region");
				ServiceLocator.Instance.Get<ILoggingService>().LogEvent(LoggingService.BeaconRegionLeftEvent);
			};

			var userId = ServiceLocator.Instance.Get<IAccountStoreService>().GetAccountUserId();
			var tupple = UtilHelper.GetMajorMinorFromUserId(userId);
			//a region constitutes the beacon that should be found. 
			var region = new AltBeaconOrg.BoundBeacon.Region("com.company.name", Identifier.Parse(UuidHelper.WakeupBeaconUuidString), Identifier.FromInt(tupple.Item1), Identifier.FromInt(tupple.Item2));
			BeaconManager.SetMonitorNotifier(_monitorNotifier);
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
