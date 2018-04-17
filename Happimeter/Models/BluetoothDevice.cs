using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using Happimeter.Services;
using Plugin.BluetoothLE;

namespace Happimeter.Models
{
    public class BluetoothDevice
    {
        private IBluetoothService _btService;
        public BluetoothDevice()
        {
            _btService = ServiceLocator.Instance.Get<IBluetoothService>();
        }

        public BluetoothDevice(IDevice device)
        {
            Device = device;
            Name = Device.Name ?? "No Name";
            Description = Device.Uuid.ToString();
        }

        public string Name { get; set; }
        public string Description { get; set; }

        protected readonly ReplaySubject<bool> InitializedReplaySubject = new ReplaySubject<bool>();

        public IDevice Device { get; set; }

        public IObservable<ConnectionStatus> OnStatusChanged() => Device.WhenStatusChanged();

        public virtual IObservable<bool> WhenDeviceReady()
        {
            return InitializedReplaySubject.AsObservable();
        }

        public virtual IObservable<object> Connect()
        {
            //var tmp = CrossBleAdapter.Current.GetConnectedDevices();
            //var tmp2 = CrossBleAdapter.Current.GetKnownDevice();
            //var tmp3 = CrossBleAdapter.Current.GetPairedDevices();
            //var tmp4 = CrossBleAdapter.AndroidOperationPause;
            //CrossBleAdapter.AndroidOperationPause = CrossBleAdapter.AndroidOperationPause.Value.Add(TimeSpan.FromMilliseconds(2000));
            var connection = Device.Connect(new GattConnectionConfig
            {

                AutoConnect = false,
                Priority = ConnectionPriority.High
            });
            connection.Subscribe(res =>
            {
                InitializedReplaySubject.OnNext(true);
            }, error =>
            {
                Debug.WriteLine(error.Message);
                //InitializedReplaySubject.OnError(new Exception("Exception while establishing connection to device"));
            });

            Device.WhenStatusChanged().Subscribe((x) =>
            {
                Debug.WriteLine(x);
            }, (err) =>
            {
                Debug.WriteLine(err);
            });

            return connection;
        }

        public virtual void SendNotification()
        {
            Debug.WriteLine("Nothin to do here");
        }

        public static BluetoothDevice Create(IDevice device)
        {
            if (device?.Name?.Contains("MI Band") ?? false)
            {
                return new MiBand2Device(device);
            }

            if (device?.Name?.Contains("Happimeter") ?? false)
            {
                return new AndroidWatch(device);
            }

            return new BluetoothDevice(device);
        }
    }
}
