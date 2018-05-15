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

namespace Happimeter.Watch.Droid.Services
{
	public class BeaconListenerService : Java.Lang.Object, IBeaconConsumer
	{
		private BeaconManager BeaconManager { get; set; }
		private Region ToMonitor { get; set; }
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
			BeaconManager.SetForegroundScanPeriod(60 * 1000);
			BeaconManager.SetBackgroundScanPeriod(60 * 1000);
			BeaconManager.SetBackgroundBetweenScanPeriod(60 * 1000);
			BeaconManager.SetForegroundBetweenScanPeriod(60 * 1000);
			PowerSave = new BackgroundPowerSaver(ApplicationContext);

			_rangeNotifier.DidRangeBeaconsInRegionComplete += _rangeNotifier_DidRangeBeaconsInRegionComplete;

			var region = new AltBeaconOrg.BoundBeacon.Region("com.company.name", Identifier.Parse(UuidHelper.BeaconUuidString), null, null);
			ToMonitor = region;

			BeaconManager.SetRangeNotifier(_rangeNotifier);
			BeaconManager.StartRangingBeaconsInRegion(region);
		}

		public void UnbindService(IServiceConnection serviceConnection)
		{
			ApplicationContext.UnbindService(serviceConnection);
		}

		public void StartListeningForBeacons()
		{
			BeaconManager.SetDebug(true);
			BeaconManager = BeaconManager.GetInstanceForApplication(Application.Context);
			var iBeaconParser = new BeaconParser();
			//  ibeacon layout
			iBeaconParser.SetBeaconLayout(UuidHelper.BeaconLayout);
			BeaconManager.BeaconParsers.Add(iBeaconParser);
			BeaconManager.Bind(this);
		}

		public void StopListeningForBeacons()
		{
			if (BeaconManager != null && ToMonitor != null)
			{
				BeaconManager.StopRangingBeaconsInRegion(ToMonitor);
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
