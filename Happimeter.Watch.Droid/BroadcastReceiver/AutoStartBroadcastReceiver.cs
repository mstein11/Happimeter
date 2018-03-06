
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
            //Log.Info("TEST", "HELLO FROM THE RECEIVER");
            Toast.MakeText(context, "Received intent!", ToastLength.Long).Show();
            var intend = new Intent(context, typeof(BackgroundService));
            intend.AddFlags(ActivityFlags.NewTask);
            Console.WriteLine("Hello");
            Log.Error("TEST", "HALLO");
            context.StartService(intend);

            /*
            var preferences = context.GetSharedPreferences("TEST", FileCreationMode.Append);
            var editor = preferences.Edit();

            editor.PutString("StartEvent", DateTime.UtcNow.ToString());
            editor.Commit();
            */
        }
    }
}
