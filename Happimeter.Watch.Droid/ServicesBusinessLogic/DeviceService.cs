using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Core.Services;
using Happimeter.Watch.Droid.Activities;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.Workers;
using System.Linq;

namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
	public class DeviceService : IDeviceService
	{
		public DeviceService()
		{
		}

		public string GetDeviceName()
		{
			var configService = ServiceLocator.Instance.Get<IConfigService>();
			var name = configService.GetConfigValueByKey(ConfigService.WatchNameKey);
			if (name == null)
			{
				name = "Happimeter " + BluetoothHelper.GetBluetoothName();
				configService.AddOrUpdateConfigEntry(ConfigService.WatchNameKey, name);
			}
			return name;
		}

		public void AddPairing(AuthSecondMessage message)
		{
			var pairedDevice = new BluetoothPairing
			{
				PhoneOs = message.PhoneOs,
				Password = message.Password,
				LastDataSync = null,
				IsPairingActive = true,
				PairedAt = DateTime.UtcNow,
				//PairedDeviceName = address,
				PairedWithUserName = message.HappimeterUsername,
				PairedWithUserId = message.HappimeterUserId
			};

			ServiceLocator.Instance.Get<IDatabaseContext>().Add(pairedDevice);
			//start beacon
			BeaconWorker.GetInstance().Start();
			BluetoothWorker.GetInstance().RemoveAuthService();
		}

		public void RemovePairing()
		{
			ServiceLocator.Instance.Get<IDatabaseContext>().DeleteAll<BluetoothPairing>();
			BeaconWorker.GetInstance().Stop();
			BluetoothWorker.GetInstance().AddAuthService();

			MeasurementWorker.GetInstance().Stop();
			MicrophoneWorker.GetInstance().Stop();
		}

		public bool IsPaired()
		{
			return ServiceLocator.Instance.Get<IDatabaseContext>().Get<BluetoothPairing>(x => x.IsPairingActive) != null;
		}

		public void NavigateToPairingRequestPage(string deviceName)
		{
			var intent = new Intent(Application.Context.ApplicationContext, typeof(PairingRequestActivity));
			intent.AddFlags(ActivityFlags.NewTask);
			intent.PutExtra("DeviceName", deviceName);
			Application.Context.StartActivity(intent);
		}

		/// <summary>
		///     Enables the Continous mode. In continous mode, the watch constantly collects sensor data. 
		///     Every minute, the watch calculated average, etc of the collected metrics and safes it to the db.
		/// </summary>
		public void SetContinousMeasurementMode()
		{
			ServiceLocator.Instance.Get<IConfigService>().SetContinousMeasurementMode();

			MeasurementWorker.GetInstance().Stop();
			MicrophoneWorker.GetInstance().Stop();
			BeaconWorker.GetInstance().Stop();
			System.Diagnostics.Debug.WriteLine("Stopped battery safer mode - Started contious mode");
			if (!MicrophoneWorker.GetInstance().IsRunning)
			{
				Task.Factory.StartNew(() =>
				{
					MicrophoneWorker.GetInstance().Start();
				});
			}

			if (!MeasurementWorker.GetInstance().IsRunning)
			{
				Task.Factory.StartNew(() =>
				{
					MeasurementWorker.GetInstance(Application.Context).Start();
				});
			}

			if (!BeaconWorker.GetInstance().IsRunning)
			{
				Task.Factory.StartNew(() =>
				{
					BeaconWorker.GetInstance().Start();
				});
			}

			if (!BluetoothScannerWorker.GetInstance().IsRunning)
			{
				Task.Factory.StartNew(() =>
				{
					BluetoothScannerWorker.GetInstance().Start();
				});
			}
		}

		/// <summary>
		///     Toggles the measurement mode of the watch.
		///     Batterysafer mode is characterized by first gathering the sensor data for half of the given interval and then doing nothing the other half of the interval.
		///     During the time where there is done nothing, the watch can go into hibernate mode to save battery.
		/// </summary>
		/// <param name="measurementInterval">Measurement interval.</param>
		public void SetBatterySaferMeasurementMode(int measurementInterval = 300)
		{
			ServiceLocator.Instance.Get<IConfigService>().SetBatterySaferMeasurementMode(measurementInterval);

			MeasurementWorker.GetInstance().Stop();
			MicrophoneWorker.GetInstance().Stop();
			BeaconWorker.GetInstance().Stop();

			System.Diagnostics.Debug.WriteLine("Stopped Continous mode - Started battery safer mode");

			var alarmManager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
			Intent alarmIntent = new Intent(Application.Context, typeof(BroadcastReceiver.AlarmBroadcastReceiver));
			var pendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, alarmIntent, 0);

			//start battery safer mode in 2 seconds via wakeup alarm
			alarmManager.SetExact(AlarmType.ElapsedRealtimeWakeup,
								  SystemClock.ElapsedRealtime() +
								  2 * 1000, pendingIntent);

			var alarmIntentBeacon = new Intent(Application.Context, typeof(BroadcastReceiver.BeaconAlarmBroadcastReceiver));
			var pendingIntentBeacon = PendingIntent.GetBroadcast(Application.Context, 0, alarmIntentBeacon, 0);
			alarmManager.SetExact(AlarmType.ElapsedRealtimeWakeup,
								  SystemClock.ElapsedRealtime() +
								  2 * 1000, pendingIntentBeacon);
		}

		/// <summary>
		///     If this method retuns null, we are in Continous Mode.
		///     If this method returns a int value. We are in BatterySaferMode and the returned values indicates the duration of one interval.
		/// </summary>
		/// <returns>The measurement mode.</returns>
		public int? GetMeasurementMode()
		{
			return ServiceLocator.Instance.Get<IConfigService>().GetMeasurementMode();
		}

		/// <summary>
		///     Helper method to identify wheater watch runs in contonous or battery saver mode.
		/// </summary>
		/// <returns><c>true</c>, if continous measurement mode was ised, <c>false</c> if runnign in battery saver mode.</returns>
		public bool IsContinousMeasurementMode()
		{
			return ServiceLocator.Instance.Get<IConfigService>().IsContinousMeasurementMode();
		}

		public BluetoothPairing GetBluetoothPairing()
		{
			return ServiceLocator.Instance.Get<IDatabaseContext>().GetAll<BluetoothPairing>().FirstOrDefault(x => x.IsPairingActive);
		}
	}
}
