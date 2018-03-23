
using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.App.Job;
using Android.Bluetooth;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.OS;
using Android.Util;
using Android.Widget;
using Happimeter.Watch.Droid.Workers;

namespace Happimeter.Watch.Droid.Services
{
    [Service(Label = "BackgroundService")]
    public class BackgroundService : Service
    {
        public static Context ServiceContext { get; set; }

        IBinder binder;
        FusedLocationProviderClient fusedLocationProviderClient;

        public override void OnDestroy()
        {
            base.OnDestroy();

            BluetoothWorker.GetInstance().Stop();
            MicrophoneWorker.GetInstance().Stop();
        }

        public override void OnCreate()
        {
            base.OnCreate();
            Toast.MakeText(this, "My Service Started", ToastLength.Long).Show();
            ServiceContext = this;
        }

        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            
            if (!MicrophoneWorker.GetInstance().IsRunning) {
                Task.Factory.StartNew(() =>
                {
                    MicrophoneWorker.GetInstance().Start();
                });    
            }

           


            if (!MeasurementWorker.GetInstance().IsRunning) {
                Task.Factory.StartNew(() =>
                {
                    MeasurementWorker.GetInstance().Start();
                });    
            }

            if (!BluetoothWorker.GetInstance().IsRunning) {
                if (BluetoothAdapter.DefaultAdapter.IsEnabled)
                {
                    Task.Factory.StartNew(() =>
                    {
                        BluetoothWorker.GetInstance().Start();
                    });
                }    
            }
            return StartCommandResult.StickyCompatibility;
        }

        public override IBinder OnBind(Intent intent)
        {
            binder = new BackgroundServiceBinder(this);
            return binder;
        }
    }

    public class BackgroundServiceBinder : Binder
    {
        readonly BackgroundService service;

        public BackgroundServiceBinder(BackgroundService service)
        {
            this.service = service;
        }

        public BackgroundService GetBackgroundService()
        {
            return service;
        }
    }

    public class FusedLocationProviderCallback : LocationCallback
    {
        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            Console.WriteLine($"IsLocationAvailable: {locationAvailability.IsLocationAvailable}");
        }

        public override void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Any())
            {
                var location = result.Locations.First();
                Console.WriteLine($"The location is :" + location.Latitude + " - " + location.Longitude);
            }
            else
            {
                // No locations to work with.
            }
        }
    }
}
