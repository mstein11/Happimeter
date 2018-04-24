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
        private static DateTime? NextScheuleTime;
        public AlarmBroadcastReceiver()
        {
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (NextScheuleTime != null && NextScheuleTime > DateTime.UtcNow)
            {
                //we don't need to do something, this is not the alarm we are listening for.
                //Alarms are retained until the device is rebooted, so if the app stops and is restarted, we might have two alarm which we don't want.
                System.Diagnostics.Debug.WriteLine("RECEIVED MEASUREMENT ALARM BEFORE SCHEDULE... IGNORING");
                return;
            }
            IsScheduled = false;
            var deviceService = ServiceLocator.Instance.Get<IDeviceService>();
            var duration = deviceService.GetMeasurementMode();
            System.Diagnostics.Debug.WriteLine("Received alarm");

            if (duration == null)
            {
                //do not reschedule alarm. do not start the workers! We are actually in continous mode already.
                System.Diagnostics.Debug.WriteLine("RECEIVED MEASUREMENT ALARM BUT WE ARE IN CONTINOUS MODE... IGNORING");
                return;
            }

            //reschedule the alarm
            var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
            Intent alarmIntent = new Intent(context, typeof(AlarmBroadcastReceiver));
            var pendingIntent = PendingIntent.GetBroadcast(context, 0, alarmIntent, 0);

            var durationAsTimespan = TimeSpan.FromSeconds(duration.Value);

            var next = DateTime.UtcNow.Add(durationAsTimespan);
            while (next.Minute % durationAsTimespan.Minutes != 0)
            {
                next = next.AddMinutes(1);
            }
            next = next.AddSeconds(next.Second * -1);
            //store when the next alarm is scheduled, so that we stop alarm that are from previous start of the app.
            NextScheuleTime = next;
            //we want to start every measurement around the 30th second.
            next = next.AddSeconds(30);

            System.Diagnostics.Debug.WriteLine(next);
            var nextAsTimeSpan = next - DateTime.UtcNow;
            alarmManager.SetExact(AlarmType.ElapsedRealtimeWakeup,
                                  SystemClock.ElapsedRealtime() +
                                  (int)nextAsTimeSpan.TotalSeconds * 1000, pendingIntent);

            //if we dont have this code, the ui tread is blocked by the microphone worker
            Task.Factory.StartNew(() =>
            {
                var measurementWorker = MeasurementWorker.GetInstance(context);
                MicrophoneWorker.GetInstance().StartFor((int)duration.Value / 5);
                measurementWorker.StartFor((int)duration.Value / 5);
                BluetoothScannerWorker.GetInstance().StartFor((int)duration.Value / 5);
            });

            IsScheduled = true;
        }
    }
}
