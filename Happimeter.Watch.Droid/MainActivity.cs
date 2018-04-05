using Android.App;
using Android.Widget;
//using Android.OS;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.OS;
using Happimeter.Watch.Droid.Services;
using Happimeter.Watch.Droid.Database;
using Happimeter.Core.Database;
using Happimeter.Watch.Droid.Activities;
using System;
using System.Linq;
using Android.Views;
using Happimeter.Watch.Droid.DependencyInjection;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace Happimeter.Watch.Droid
{
    [Activity(Label = "Happimeter.Watch.Droid", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity, ISensorEventListener
    {
        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            System.Diagnostics.Debug.WriteLine(accuracy);
        }

        public void OnSensorChanged(SensorEvent e)
        {
            Console.WriteLine(e.Values.FirstOrDefault());
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Container.RegisterElements();
            AppCenter.Start("a614a5b2-5aeb-47ac-a4e9-1256a337a0b7",
                typeof(Analytics), typeof(Crashes));

            RequestWindowFeature(WindowFeatures.NoTitle);
            //Remove notification bar
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);


            var pairing = ServiceLocator.Instance.Get<IDatabaseContext>().GetCurrentBluetoothPairing();
            if (pairing != null)
            {

            }
            else
            {
                var intent = new Intent(this, typeof(PairingActivity));
                StartActivity(intent);
                Finish();
            }

            FindViewById<Button>(Resource.Id.removePairingButton).Click += delegate
            {
                ServiceLocator.Instance.Get<IDeviceService>().RemovePairing();
                var intent = new Intent(this, typeof(PairingActivity));
                StartActivity(intent);
                Finish();
            };
            FindViewById<Button>(Resource.Id.surveyButton).Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(SurveyActivity));
                StartActivity(intent);
            };
            if (!IsMyServiceRunning(typeof(BackgroundService)))
            {
                StartService(new Intent(this, typeof(BackgroundService)));
            }
            if (!IsMyServiceRunning(typeof(BeaconService)))
            {
                StartService(new Intent(this, typeof(BeaconService)));
            }
        }

        private bool IsMyServiceRunning(Type serviceClass)
        {
            var manager = (ActivityManager)GetSystemService(Context.ActivityService);
            foreach (var service in manager.GetRunningServices(int.MaxValue))
            {
                if (service.GetType() == serviceClass)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

