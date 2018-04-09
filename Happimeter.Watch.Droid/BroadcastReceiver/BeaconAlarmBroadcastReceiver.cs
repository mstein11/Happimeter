
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Happimeter.Watch.Droid.Workers;

namespace Happimeter.Watch.Droid.BroadcastReceiver
{
    [BroadcastReceiver]
    public class BeaconAlarmBroadcastReceiver : Android.Content.BroadcastReceiver
    {
        public static bool IsScheduled = false;

        public override void OnReceive(Context context, Intent intent)
        {
            IsScheduled = false;
            var deviceService = ServiceLocator.Instance.Get<IDeviceService>();
            var isContinous = deviceService.IsContinousMeasurementMode();
            System.Diagnostics.Debug.WriteLine("Received alarm");

            if (isContinous)
            {
                //do not reschedule alarm. do not start the workers! We are actually in continous mode already.
                return;
            }

            //reschedule the alarm
            var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
            Intent alarmIntent = new Intent(context, typeof(BeaconAlarmBroadcastReceiver));
            var pendingIntent = PendingIntent.GetBroadcast(context, 0, alarmIntent, 0);


            //we don't do exact here. it is not so important that we have exactly 20 minutes intervals
            alarmManager.Set(AlarmType.ElapsedRealtimeWakeup,
                                  SystemClock.ElapsedRealtime() +
                             BluetoothHelper.BeaconPeriodInSeconds * 1000, pendingIntent);

            //if we dont have this code, the ui tread is blocked by the microphone worker
            Task.Factory.StartNew(() =>
            {
                var beaconWorker = BeaconWorker.GetInstance();
                if (beaconWorker.IsRunning)
                {
                    //if its running we stop here and wait 20 minutes (until next alarm fires)
                    beaconWorker.Stop();
                }
                else
                {
                    //if it is stopped we start it. In the next alarm it will be stopped then.
                    beaconWorker.Start();
                }
            });

            IsScheduled = true;
        }
    }
}
