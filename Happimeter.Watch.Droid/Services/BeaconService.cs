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
    [Service(Label = "BeaconService")]
    public class BeaconService : Service
    {
        IBinder binder;

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (!BluetoothAdapter.DefaultAdapter.IsEnabled)
            {
                Toast.MakeText(Application.Context, "Bluetooth is not activated", ToastLength.Long).Show();
                return base.OnStartCommand(intent, flags, startId);;
            }
            Task.Factory.StartNew(() =>
            {
                BeaconWorker.GetInstance().Start();
            });

            return base.OnStartCommand(intent, flags, startId);

        }

        public override IBinder OnBind(Intent intent)
        {
            binder = new BeaconServiceBinder(this);
            return binder;
        }
    }

    public class BeaconServiceBinder : Binder
    {
        readonly BeaconService service;

        public BeaconServiceBinder(BeaconService service)
        {
            this.service = service;
        }

        public BeaconService GetBackgroundService()
        {
            return service;
        }
    }
}
