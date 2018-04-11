using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Happimeter.Watch.Droid.Workers;

namespace Happimeter.Watch.Droid.BroadcastReceiver
{
    [BroadcastReceiver(Enabled = true)]
    public class AlarmBroadcastReceiver : Android.Content.BroadcastReceiver
    {
        public static bool IsScheduled = false;
        public AlarmBroadcastReceiver()
        {
        }

        public override void OnReceive(Context context, Intent intent)
        {
            IsScheduled = false;
            var deviceService = ServiceLocator.Instance.Get<IDeviceService>();
            var duration = deviceService.GetMeasurementMode();
            System.Diagnostics.Debug.WriteLine("Received alarm");

            if (duration == null)
            {
                //do not reschedule alarm. do not start the workers! We are actually in continous mode already.
                return;
            }

            //reschedule the alarm
            var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
            Intent alarmIntent = new Intent(context, typeof(AlarmBroadcastReceiver));
            var pendingIntent = PendingIntent.GetBroadcast(context, 0, alarmIntent, 0);


            alarmManager.SetExact(AlarmType.ElapsedRealtimeWakeup,
                                  SystemClock.ElapsedRealtime() +
                                  duration.Value * 1000, pendingIntent);

            //if we dont have this code, the ui tread is blocked by the microphone worker
            Task.Factory.StartNew(() =>
            {
                var measurementWorker = MeasurementWorker.GetInstance(context);
                MicrophoneWorker.GetInstance().StartFor((int)duration.Value / 2);
                measurementWorker.StartFor((int)duration.Value / 2);
                BluetoothScannerWorker.GetInstance().StartFor((int)duration.Value / 2);
            });

            IsScheduled = true;
        }
    }
}
