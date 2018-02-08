using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Bluetooth;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.Workers;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
    public class HappimeterAuthCharacteristic : BluetoothGattCharacteristic
    {
        public const string CharacteristicUuid = "68b13553-0c4d-43de-8c1c-2b10d77d2d90";
        private const GattProperty GattProperties = GattProperty.Read | GattProperty.Write | GattProperty.Notify;
        private const GattPermission GattPermissions = GattPermission.ReadEncrypted | GattPermission.WriteEncrypted;
        private Dictionary<string, bool> AuthenticationDeviceDidGreat = new Dictionary<string, bool>();
        private Dictionary<string, bool> AuthenticationDeviceDidReceiveCreds = new Dictionary<string, bool>();

        public HappimeterAuthCharacteristic() : base(uuid: UUID.FromString(CharacteristicUuid), properties: GattProperties, permissions: GattPermissions)
        {
        }

        public void HandleRead(BluetoothDevice device, int requestId, int offset, BluetoothWorker worker) {
            if (!AuthenticationDeviceDidGreat.ContainsKey(device.Address)) {
                System.Diagnostics.Debug.WriteLine($"Device {device.Address} read from auth characteristic without greeting first!");
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Failure, offset, Encoding.UTF8.GetBytes("Did not greet probably!"));
                return;
            }
            var resultObj = new
            {
                Uuid = BluetoothWorker.BeaconUuid,
                Minor = 1,
                Major = 0,
                Password = "pass"
            };

            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(resultObj);
            //var jsonString = string.Format("{{'UuId':'{0}', 'Minor':{1}, 'Major':{2} }}", BluetoothWorker.BeaconUuid, BluetoothWorker.Minor, BluetoothWorker.Major);

            var bytes = Encoding.UTF8.GetBytes(jsonString);
            if (!AuthenticationDeviceDidReceiveCreds.ContainsKey(device.Address))
            {
                AuthenticationDeviceDidReceiveCreds.Add(device.Address, true);
            }

            worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, bytes);
        }

        public void HandleWrite(BluetoothDevice device, int requestId, bool preparedWrite, bool responseNeeded, int offset, byte[] value, BluetoothWorker worker)
        {
            if (!value.SequenceEqual(Encoding.UTF8.GetBytes("Hallo")) && !value.SequenceEqual(Encoding.UTF8.GetBytes("Ok")))
            {
                System.Diagnostics.Debug.WriteLine($"Device {device.Address} wrote something which I don't know how to handle to auth characteristic!");
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Failure, offset, Encoding.UTF8.GetBytes("Did not greet probably!"));
                return;
            }

            //initiate auth process
            if (value.SequenceEqual(Encoding.UTF8.GetBytes("Hallo"))) {
                System.Diagnostics.Debug.WriteLine($"Device {device.Address} started authentication procedure");
                if (!AuthenticationDeviceDidGreat.ContainsKey(device.Address))
                {
                    AuthenticationDeviceDidGreat.Add(device.Address, true);
                }
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, value);
                return;
            }

            //finalize auth process
            if (value.SequenceEqual(Encoding.UTF8.GetBytes("Ok")))
            {
                System.Diagnostics.Debug.WriteLine($"Device {device.Address} finalized authentication procedure");
                //todo: save to database
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, value);
                var pairedDevice = new BluetoothPairing
                {
                    IsIosPaired = true,
                    LastDataSync = null,
                    IsPairingActive = true,
                    PairedAt = DateTime.UtcNow,
                    PairedDeviceName = device.Name,
                    PairedWithUserName = ""
                };

                ServiceLocator.Instance.Get<IDatabaseContext>().AddNewPairing(pairedDevice);

                worker.RunBeacon();
                return;
            }
        }

    }
}
