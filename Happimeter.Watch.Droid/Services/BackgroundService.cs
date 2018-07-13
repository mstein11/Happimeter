
using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.App.Job;
using Android.Bluetooth;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.OS;
using Android.Util;
using Android.Widget;
using Happimeter.Watch.Droid.Workers;
using Happimeter.Core.Helper;
using Happimeter.Core.Services;
using Happimeter.Watch.Droid.ServicesBusinessLogic;

namespace Happimeter.Watch.Droid.Services
{
	[Service(Label = "BackgroundService")]
	public class BackgroundService : Service
	{
		public static Context ServiceContext { get; set; }

		IBinder binder;

		public override void OnDestroy()
		{
			base.OnDestroy();

			BluetoothWorker.GetInstance().Stop();
			MicrophoneWorker.GetInstance().Stop();
		}

		public override void OnCreate()
		{
			base.OnCreate();
			Toast.MakeText(this, "My Service Started", ToastLength.Long).Show();
			ServiceContext = this;
		}

		public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
		{
			if (ServiceLocator.Instance.Get<IDeviceService>().IsPaired())
			{
				var deviceService = ServiceLocator.Instance.Get<IDeviceService>();
				var useLifeMode = deviceService.IsContinousMeasurementMode();

				var nextPollTime = UtilHelper.GetNextSurveyPromptTime() - DateTime.UtcNow.ToLocalTime();
				var context = Application.Context;
				var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
				Intent surveyAlarmIntent = new Intent(context, typeof(BroadcastReceiver.SurveyAlarmBroadcastReceiver));
				var surveyPendingIntent = PendingIntent.GetBroadcast(context, 0, surveyAlarmIntent, 0);

				alarmManager.Set(AlarmType.ElapsedRealtimeWakeup,
									  SystemClock.ElapsedRealtime() +
								 (long)nextPollTime.TotalMilliseconds, surveyPendingIntent);
				System.Diagnostics.Debug.WriteLine($"Scheduled new survey for: {(DateTime.UtcNow.ToLocalTime() + nextPollTime)}");
				if (useLifeMode)
				{
					System.Diagnostics.Debug.WriteLine("Starting in continous mode");
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
							MeasurementWorker.GetInstance(this).Start();
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
				else
				{

					if (!BroadcastReceiver.AlarmBroadcastReceiver.IsScheduled)
					{
						System.Diagnostics.Debug.WriteLine("Starting in battery safer mode!");
						Intent alarmIntent = new Intent(context, typeof(BroadcastReceiver.AlarmBroadcastReceiver));
						var pendingIntent = PendingIntent.GetBroadcast(context, 0, alarmIntent, 0);


						alarmManager.SetExact(AlarmType.ElapsedRealtimeWakeup,
											  SystemClock.ElapsedRealtime() +
											  2 * 1000, pendingIntent);
					}
					else
					{
						System.Diagnostics.Debug.WriteLine("already scheduled - do not start again!");
					}
				}
			}

			if (!BluetoothWorker.GetInstance().IsRunning)
			{
				if (BluetoothAdapter.DefaultAdapter.IsEnabled)
				{
					Task.Factory.StartNew(() =>
					{
						BluetoothWorker.GetInstance().Start();
					});
				}
			}
			return StartCommandResult.StickyCompatibility;
		}

		public override IBinder OnBind(Intent intent)
		{
			binder = new BackgroundServiceBinder(this);
			return binder;
		}
	}

	public class BackgroundServiceBinder : Binder
	{
		readonly BackgroundService service;

		public BackgroundServiceBinder(BackgroundService service)
		{
			this.service = service;
		}

		public BackgroundService GetBackgroundService()
		{
			return service;
		}
	}
}
