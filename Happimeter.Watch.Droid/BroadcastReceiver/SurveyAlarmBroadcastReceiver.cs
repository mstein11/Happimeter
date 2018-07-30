
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.Activities;

namespace Happimeter.Watch.Droid.BroadcastReceiver
{
    [BroadcastReceiver]
    public class SurveyAlarmBroadcastReceiver : Android.Content.BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var activityIntent = new Intent(context, typeof(PreSurveyActivity));
            activityIntent.PutExtra("IsAlarmTriggered", true);
            activityIntent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(activityIntent);

            var nextPollTime = UtilHelper.GetNextSurveyPromptTime() - DateTime.UtcNow.ToLocalTime();
            //if for some reason we have a negative timespan, lets simply set it to 2 hours
            if (nextPollTime < TimeSpan.Zero)
            {
                nextPollTime = TimeSpan.FromHours(2);
            }
            var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
            Intent surveyAlarmIntent = new Intent(context, typeof(BroadcastReceiver.SurveyAlarmBroadcastReceiver));
            var surveyPendingIntent = PendingIntent.GetBroadcast(context, 0, surveyAlarmIntent, 0);
            System.Diagnostics.Debug.WriteLine($"Scheduled new survey for: {(DateTime.UtcNow.ToLocalTime() + nextPollTime)}");
            alarmManager.Set(AlarmType.ElapsedRealtimeWakeup,
                                  SystemClock.ElapsedRealtime() +
                             (long)nextPollTime.TotalMilliseconds, surveyPendingIntent);
        }
    }
}
