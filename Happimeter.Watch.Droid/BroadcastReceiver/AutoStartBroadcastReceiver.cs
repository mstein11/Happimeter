
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
using Happimeter.Watch.Droid.DependencyInjection;
using Happimeter.Watch.Droid.Services;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Happimeter.Core.Helper;
using Happimeter.Core.Services;

namespace Happimeter.Watch.Droid
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted }, Priority = (int)IntentFilterPriority.HighPriority, Categories = new[] { "android.intent.category.DEFAULT" })]
    public class AutoStartBroadcastReceiver : Android.Content.BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Container.RegisterElements();
            var deactivatedStartOnBoot = ServiceLocator.Instance.Get<IConfigService>().GetDeactivateAppStartsOnBoot();
            if (deactivatedStartOnBoot)
            {
                return;
            }
            Toast.MakeText(context, "Received intent!", ToastLength.Long).Show();
            var backgroundIntend = new Intent(context, typeof(MainActivity));
            backgroundIntend.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(backgroundIntend);
        }
    }
}
