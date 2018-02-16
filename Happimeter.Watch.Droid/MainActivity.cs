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

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var smm = (SensorManager)GetSystemService(Context.SensorService);
            var sensorsList = smm.GetSensorList(SensorType.All);
            var acc = smm.GetDefaultSensor(SensorType.Accelerometer);
            //smm.RegisterListener(this, acc, SensorDelay.Normal);


            var pairing = ServiceLocator.Instance.Get<IDatabaseContext>().GetCurrentBluetoothPairing();
            if (pairing != null) {
                FindViewById<TextView>(Resource.Id.isPairedValue).Text = "yes";
                FindViewById<TextView>(Resource.Id.ExchangeAtValue).Text = pairing.LastDataSync?.ToString() ?? "-";
                FindViewById<TextView>(Resource.Id.PairedAtValue).Text = pairing.PairedAt?.ToString() ?? "-";
                FindViewById<Button>(Resource.Id.removePairingButton).Click += delegate {
                    ServiceLocator.Instance.Get<IDatabaseContext>().DeleteAll<BluetoothPairing>();
                    Toast.MakeText(this, "Deleted, View will not update unless you restart app",ToastLength.Long);
                };
            } else {
                FindViewById<Button>(Resource.Id.removePairingButton).Visibility = Android.Views.ViewStates.Invisible;
            }
            FindViewById<Button>(Resource.Id.surveyButton).Click += (sender, e) => {
                var intent = new Intent(this, typeof(SurveyActivity));
                StartActivity(intent);
            };

            StartService(new Intent(this,typeof(BackgroundService)));
            StartService(new Intent(this, typeof(BeaconService)));
            var tmp1 = smm.GetDefaultSensor(SensorType.HeartRate);
            smm.RegisterListener(this, tmp1, SensorDelay.Ui);
            var tmp2 = smm.GetDefaultSensor(SensorType.HeartRate);
            var heartRate2 = smm.GetDefaultSensor(SensorType.HeartBeat);

            FindViewById<Button>(Resource.Id.restartWorker).Click += (sender, e) => {
                StopService(new Intent(this, typeof(BackgroundService)));
                StartService(new Intent(this, typeof(BackgroundService)));
            };
            //RequestPermissions(Manifest.Permission.BodySensors, 0);
            //smm.RegisterListener(this, heartRate, SensorDelay.Fastest);

            //button.Click += delegate { button.Text = $"{count++} clicks!"; };
        }
    }
}

