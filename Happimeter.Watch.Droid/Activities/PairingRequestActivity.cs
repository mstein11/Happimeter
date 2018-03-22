
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
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.Workers;

namespace Happimeter.Watch.Droid.Activities
{
    [Activity(Label = "PairingRequestActivity")]
    public class PairingRequestActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature(WindowFeatures.NoTitle);

            //Remove notification bar
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            // Create your application here
            SetContentView(Resource.Layout.PairingRequest);

            var vibrator = (Vibrator) GetSystemService(Context.VibratorService);
            //vibrate for 500 milis
            vibrator.Vibrate(500);
            var deviceName = Intent.GetStringExtra("DeviceName");

            var textView = FindViewById<TextView>(Resource.Id.PairingDeviceName);
            textView.Text = deviceName;

            var acceptButton = FindViewById<Button>(Resource.Id.PairingRequestAccept);
            var declineButton = FindViewById<Button>(Resource.Id.PairingRequestDecline);
            var loading = FindViewById<ProgressBar>(Resource.Id.PairingRequestLoading);

            acceptButton.Click += (sender, e) => {
                acceptButton.Visibility = ViewStates.Gone;
                declineButton.Visibility = ViewStates.Gone;
                loading.Visibility = ViewStates.Visible;
                BluetoothWorker.GetInstance().SendNotifiation(null, new AuthNotificationMessage(true));

            };
            declineButton.Click += (sender, e) => {
                acceptButton.Visibility = ViewStates.Gone;
                declineButton.Visibility = ViewStates.Gone;
                loading.Visibility = ViewStates.Visible;
                BluetoothWorker.GetInstance().SendNotifiation(null, new AuthNotificationMessage(false));
                Finish();
            };
        }
    }
}
