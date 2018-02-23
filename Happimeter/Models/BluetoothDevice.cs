using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

        public virtual IObservable<bool> WhenDeviceReady() {
            return InitializedReplaySubject.AsObservable();
        }

        public virtual IObservable<object> Connect() {
            var connection = Device.Connect();
            connection.Subscribe(res => {
                InitializedReplaySubject.OnNext(true);
            }, error => {
                InitializedReplaySubject.OnError(new Exception("Exception while establishing connection to device"));
            });
            return connection;
        }

        public virtual void SendNotification() {
            Debug.WriteLine("Nothin to do here");
        }

        public static BluetoothDevice Create(IDevice device) {
            if (device?.Name?.Contains("MI Band") ?? false) {                
                return new MiBand2Device(device);
            }

            if (device?.Name?.Contains("Happimeter") ?? false) {
                return new AndroidWatch(device);
            }

            return new BluetoothDevice(device);
        }
    }
}
