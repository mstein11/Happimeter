using System;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Widget;
using Happimeter.Core.Helper;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Happimeter.Watch.Droid.Workers;
using Happimeter.Watch.Droid.BroadcastReceiver;

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
            var user = ServiceLocator.Instance.Get<IDatabaseContext>().Get<BluetoothPairing>(x => x.IsPairingActive == true);
            if (!BluetoothAdapter.DefaultAdapter.IsEnabled)
            {
                Toast.MakeText(Application.Context, "Bluetooth is not activated", ToastLength.Long).Show();
                return base.OnStartCommand(intent, flags, startId); ;
            }
            if (user == null)
            {
                //we don't want to start the beacon if not paired
                return base.OnStartCommand(intent, flags, startId);
            }

            var deviceService = ServiceLocator.Instance.Get<IDeviceService>();
            var useLifeMode = deviceService.IsContinousMeasurementMode();
            if (useLifeMode)
            {
                Task.Factory.StartNew(() =>
                {
                    BeaconWorker.GetInstance().Start();
                });
            }
            else
            {
                if (!BeaconAlarmBroadcastReceiver.IsScheduled)
                {
                    System.Diagnostics.Debug.WriteLine("Starting in battery safer mode!");
                    var context = Application.Context;
                    var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
                    Intent alarmIntent = new Intent(context, typeof(BroadcastReceiver.BeaconAlarmBroadcastReceiver));
                    var pendingIntent = PendingIntent.GetBroadcast(context, 0, alarmIntent, 0);


                    alarmManager.SetExact(AlarmType.ElapsedRealtimeWakeup,
                                          SystemClock.ElapsedRealtime() +
                                          2 * 1000, pendingIntent);
                }
            }

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
