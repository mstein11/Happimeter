using System;
using Android.Content;

namespace Happimeter.Watch.Droid.BroadcastReceiver
{
    [BroadcastReceiver(Enabled = true)]
    public class AlarmBroadcastReceiver : Android.Content.BroadcastReceiver
    {
        public AlarmBroadcastReceiver()
        {
        }

        public override void OnReceive(Context context, Intent intent)
        {
            throw new NotImplementedException();
        }
    }
}
