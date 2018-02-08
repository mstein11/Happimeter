using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Bluetooth;
using Happimeter.Watch.Droid.Workers;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
    public class HappimeterDataCharacteristic : BluetoothGattCharacteristic
    {
        public const string CharacteristicUuid = "7918ec07-2ba4-4542-aa13-0a10ff3826ba";
        private const GattProperty GattProperties = GattProperty.Read | GattProperty.Write | GattProperty.Notify;
        private const GattPermission GattPermissions = GattPermission.Read | GattPermission.Write;

        private Dictionary<string, bool> DidSendPass = new Dictionary<string, bool>();
        private Dictionary<string, int> ReadPosition = new Dictionary<string, int>();

        public HappimeterDataCharacteristic() : base(uuid: UUID.FromString(CharacteristicUuid), properties: GattProperties, permissions: GattPermissions)
        {
            var uuid = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
            this.WriteType = GattWriteType.Default;
            var configDescriptor = new BluetoothGattDescriptor(uuid, GattDescriptorPermission.Read | GattDescriptorPermission.Write);
            AddDescriptor(configDescriptor);
        }

        public void HandleRead(BluetoothDevice device, int requestId, int offset, BluetoothWorker worker)
        {
            if (!DidSendPass.ContainsKey(device.Address))
            {
                System.Diagnostics.Debug.WriteLine($"Device {device.Address} read from auth characteristic without authenticating first first!");
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Failure, offset, Encoding.UTF8.GetBytes("Did not authenticated!"));
                return;
            }

            var tmpData = new int[1000];
            for (var i = 0; i < tmpData.Length; i++) {
                tmpData[i] = 1;
            }

            var resultObj = new
            {
                //Uuid = BluetoothWorker.BeaconUuid,
                Data = tmpData,
                Major = 0,
                Password = "pass"
            };

            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(resultObj);
            //var jsonString = string.Format("{{'UuId':'{0}', 'Minor':{1}, 'Major':{2} }}", BluetoothWorker.BeaconUuid, BluetoothWorker.Minor, BluetoothWorker.Major);
            Debug.WriteLine("Reading");
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            if (bytes.Length > 20) {
                if (!ReadPosition.ContainsKey(device.Address)) {
                    ReadPosition.Add(device.Address, 0);
                }

                var bytesToSend = bytes.Skip((20 * ReadPosition[device.Address])).Take(20);
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, bytesToSend.ToArray());
                ReadPosition[device.Address]++;
            }

        }

        public async Task HandleWriteAsync(BluetoothDevice device, int requestId, bool preparedWrite, bool responseNeeded, int offset, byte[] value, BluetoothWorker worker)
        {
            if (!value.SequenceEqual(Encoding.UTF8.GetBytes("pass")))
            {
                System.Diagnostics.Debug.WriteLine($"Device {device.Address} wrote something which I don't know how to handle to data characteristic!");
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Failure, offset, Encoding.UTF8.GetBytes("wrong pass phrase!"));
                return;
            }

            //initiate auth process
            if (value.SequenceEqual(Encoding.UTF8.GetBytes("pass")))
            {
                System.Diagnostics.Debug.WriteLine($"Device {device.Address} authenticated with passphrase");
                if (!DidSendPass.ContainsKey(device.Address))
                {
                    DidSendPass.Add(device.Address, true);
                }
                if (ReadPosition.ContainsKey(device.Address))
                {
                    //reset previous read attempts
                    ReadPosition[device.Address] = 0;
                }
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, value);
            }
        }
    }
}
