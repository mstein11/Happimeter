
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Happimeter.Watch.Droid.Services;

namespace Happimeter.Watch.Droid
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted }, Priority = (int)IntentFilterPriority.LowPriority)]
    public class AutoStartBroadcastReceiver : Android.Content.BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(context, "Received intent!", ToastLength.Long).Show();
            var backgroundIntend = new Intent(context, typeof(BackgroundService));
            //backgroundIntend.AddFlags(ActivityFlags.NewTask);
            context.StartService(backgroundIntend);

            var beaconIntend = new Intent(context, typeof(BeaconService));
            //beaconIntend.AddFlags(ActivityFlags.NewTask);
            context.StartService(beaconIntend);
        }
    }
}
