
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
            Task.Factory.StartNew(() =>
            {
                MicrophoneWorker.GetInstance().Start();
            });
            Task.Factory.StartNew(() =>
            {
                MeasurementWorker.GetInstance().Start();
            });

            if (BluetoothAdapter.DefaultAdapter.IsEnabled)
            {                
                Task.Factory.StartNew(() =>
                {
                    BluetoothWorker.GetInstance().Start();
                });
            }
            // start your service logic here
            Task.Factory.StartNew(async () =>
            {
                var preferences = GetSharedPreferences("TEST", FileCreationMode.Append);

                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    var numberOfEntries = preferences.GetInt("NumberOfEntries", 0);
                    numberOfEntries++;
                    var timeStamp = DateTime.UtcNow.ToString();
                    var editor = preferences.Edit();
                    editor.PutInt("NumberOfEntries", numberOfEntries);
                    editor.PutString("LastTime", timeStamp);
                    editor.Commit();
                }
            });

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
