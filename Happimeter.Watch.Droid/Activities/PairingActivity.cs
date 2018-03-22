
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.ServicesBusinessLogic;

namespace Happimeter.Watch.Droid.Activities
{
    [Activity(Label = "PairingActivity")]
    public class PairingActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);

            //Remove notification bar
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            // Create your application here
            SetContentView(Resource.Layout.Pairing);
            var db = ServiceLocator.Instance.Get<IDatabaseContext>();
            var name = ServiceLocator.Instance.Get<IDeviceService>().GetDeviceName();

            var textView = FindViewById<TextView>(Resource.Id.PairingDeviceName);
            textView.Text = name;

            db.WhenEntryChanged<BluetoothPairing>().Take(1).Subscribe(eventInfo => {
                if (eventInfo.Entites.Cast<BluetoothPairing>().Any(x => x.IsPairingActive)) {
                    var intent = new Intent(this, typeof(MainActivity));
                    StartActivity(intent);
                    Finish();
                }
            });
            // Create your application here
        }
    }
}
