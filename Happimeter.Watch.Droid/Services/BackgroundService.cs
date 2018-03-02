
using System;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Widget;
using Happimeter.Watch.Droid.Workers;

namespace Happimeter.Watch.Droid.Services
{
    [Service(Label = "BackgroundService")]
    public class BackgroundService : Service
    {
        IBinder binder;

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
            var preferences = GetSharedPreferences("TEST", FileCreationMode.Append);
            var editor = preferences.Edit();

            editor.PutString("LastStarted", DateTime.UtcNow.ToString());
            editor.Commit();
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
}
