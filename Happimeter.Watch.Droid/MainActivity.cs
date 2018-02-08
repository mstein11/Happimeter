using Android.App;
using Android.Widget;
//using Android.OS;
using System.Linq;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using System.Diagnostics;
using Android.OS;
using Android;
using Android.Media;
using System.Threading.Tasks;
using System.IO;
using System;
using Happimeter.Watch.Droid.Services;
using Happimeter.Watch.Droid.Database;

namespace Happimeter.Watch.Droid
{
    [Activity(Label = "Happimeter.Watch.Droid", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity, ISensorEventListener
    {
        int count = 1;

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            System.Diagnostics.Debug.WriteLine(accuracy);
        }

        public void OnSensorChanged(SensorEvent e)
        {
            FindViewById<TextView>(Resource.Id.AccX).Text = e.Values[0].ToString();
            FindViewById<TextView>(Resource.Id.AccY).Text = e.Values[1].ToString();
            FindViewById<TextView>(Resource.Id.AccZ).Text = e.Values[2].ToString();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var smm = (SensorManager)GetSystemService(Context.SensorService);
            var sensorsList = smm.GetSensorList(SensorType.All);
            var acc = smm.GetDefaultSensor(SensorType.Accelerometer);
            smm.RegisterListener(this, acc, SensorDelay.Normal);


            var preferences = GetSharedPreferences("TEST", FileCreationMode.Append);

            var numberOfEntries = FindViewById<TextView>(Resource.Id.NumberOfEntries);
            var timeStamp = FindViewById<TextView>(Resource.Id.TimeStamp);

            numberOfEntries.Text = preferences.GetInt("NumberOfEntries", 0).ToString();
            timeStamp.Text = preferences.GetString("LastTime", "0");

            var lastStopped = preferences.GetString("LastStopped", "0");
            var lastStarted = preferences.GetString("LastStarted", "0");
            var start = preferences.GetString("StartEvent", "0");


            var measurements = ServiceLocator.Instance.Get<IDatabaseContext>().GetMicrophoneMeasurements();
            var pairing = ServiceLocator.Instance.Get<IDatabaseContext>().GetCurrentBluetoothPairing();
            if (pairing != null) {
                FindViewById<TextView>(Resource.Id.isPairedValue).Text = "yes";
                FindViewById<TextView>(Resource.Id.ExchangeAtValue).Text = pairing.LastDataSync?.ToString() ?? "-";
                FindViewById<TextView>(Resource.Id.PairedAtValue).Text = pairing.PairedAt?.ToString() ?? "-";
                FindViewById<Button>(Resource.Id.removePairingButton).Click += delegate {
                    ServiceLocator.Instance.Get<IDatabaseContext>().DeleteAllBluetoothPairings();
                    Toast.MakeText(this, "Deleted, View will not update unless you restart app",ToastLength.Long);
                };
            } else {
                FindViewById<Button>(Resource.Id.removePairingButton).Visibility = Android.Views.ViewStates.Invisible;
            }
            System.Diagnostics.Debug.WriteLine(string.Concat(measurements));

            StartService(new Intent(this,typeof(BackgroundService)));

            var heartRate2 = smm.GetDefaultSensor(SensorType.HeartBeat);
            //RequestPermissions(Manifest.Permission.BodySensors, 0);
            //smm.RegisterListener(this, heartRate, SensorDelay.Fastest);

            //button.Click += delegate { button.Text = $"{count++} clicks!"; };
        }
    }
}

