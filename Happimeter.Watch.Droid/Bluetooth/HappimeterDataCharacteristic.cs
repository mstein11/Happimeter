using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
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
        private Dictionary<string, string> JsonForDevice = new Dictionary<string, string>();

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
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, Encoding.UTF8.GetBytes("Did not authenticated!"));
                return;
            }

            if (!JsonForDevice.ContainsKey(device.Address)) {
                var toTransfer = ServiceLocator.Instance.Get<IMeasurementService>().GetMeasurementsForDataTransfer();
                var jsonStringToTransfer = Newtonsoft.Json.JsonConvert.SerializeObject(toTransfer);
                JsonForDevice.Add(device.Address, jsonStringToTransfer);
            }



            var jsonString = JsonForDevice[device.Address];

            //var jsonString = string.Format("{{'UuId':'{0}', 'Minor':{1}, 'Major':{2} }}", BluetoothWorker.BeaconUuid, BluetoothWorker.Minor, BluetoothWorker.Major);
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            var mtu = worker.DevicesMtu.ContainsKey(device.Address) ? worker.DevicesMtu[device.Address] : 20;


/*            if (bytes.Length > mtu) {*/

            if (!ReadPosition.ContainsKey(device.Address)) {
                ReadPosition.Add(device.Address, 0);
            }
            var readPos = ReadPosition[device.Address];
            var bytesToSend = bytes.Skip(readPos).Take(mtu).ToList();
            if (bytesToSend.Count() == 0) {
                bytesToSend = bytesToSend.ToList();
                //if we send an empty response, the client will crash, so we add one byte to the response
                bytesToSend.Add(0x00);
            }
            worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, bytesToSend.ToArray());
            ReadPosition[device.Address] += mtu;
/*            } else {
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, bytes.ToArray());
            }*/

        }

        public async Task HandleWriteAsync(BluetoothDevice device, int requestId, bool preparedWrite, bool responseNeeded, int offset, byte[] value, BluetoothWorker worker)
        {
            var messageRaw = Encoding.UTF8.GetString(value);
            var message = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseBluetoothMessage>(Encoding.UTF8.GetString(value));

            if (message.MessageName != nameof(DataExchangeFirstMessage) && message.MessageName != nameof(DataExchangeConfirmationMessage))
            {
                System.Diagnostics.Debug.WriteLine($"Device {device.Address} wrote something which I don't know how to handle to data characteristic!");
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, Encoding.UTF8.GetBytes("wrong pass phrase!"));
                return;
            }

            //initiate auth process
            if (message.MessageName == nameof(DataExchangeFirstMessage))
            {
                System.Diagnostics.Debug.WriteLine($"Device {device.Address} authenticated with passphrase");
                if (!DidSendPass.ContainsKey(device.Address))
                {
                    DidSendPass.Add(device.Address, true);
                }
                ResetStateForDevice(device.Address);
            }

            if (message.MessageName == nameof(DataExchangeConfirmationMessage) && JsonForDevice.ContainsKey(device.Address)) {
                var messageToDelete = Newtonsoft.Json.JsonConvert.DeserializeObject<DataExchangeMessage>(JsonForDevice[device.Address]);
                ServiceLocator.Instance.Get<IMeasurementService>().DeleteSurveyMeasurement(messageToDelete);
                ResetStateForDevice(device.Address);
            }

            if (responseNeeded) {
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, value);
            }
        }

        private void ResetStateForDevice(string address) {
            if (ReadPosition.ContainsKey(address))
            {
                //reset previous read attempts
                ReadPosition[address] = 0;
            }
            if (JsonForDevice.ContainsKey(address))
            {
                JsonForDevice.Remove(address);
            }
        }
    }
}
