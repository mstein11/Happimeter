using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Happimeter.Watch.Droid.Workers;
using Java.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Happimeter.Watch.Droid.Bluetooth
{
    public class HappimeterDataCharacteristic : BluetoothGattCharacteristic
    {
        private const GattProperty GattProperties = GattProperty.Read | GattProperty.Write | GattProperty.Notify;
        private const GattPermission GattPermissions = GattPermission.Read | GattPermission.Write;

        private Dictionary<string, bool> DidSendPass = new Dictionary<string, bool>();
        private Dictionary<string, int> ReadPosition = new Dictionary<string, int>();
        private Dictionary<string, string> JsonForDevice = new Dictionary<string, string>();

        private Dictionary<string, WriteReceiverContext> WriteReceiverContextForDevice = new Dictionary<string, WriteReceiverContext>();
        private Dictionary<string, ReadHostContext> ReadHostContextForDevice = new Dictionary<string, ReadHostContext>();


        public HappimeterDataCharacteristic() : base(uuid: UUID.FromString(UuidHelper.DataExchangeCharacteristicUuidString), properties: GattProperties, permissions: GattPermissions)
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
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset,
                                               new byte[] { 0x00, 0x00, 0x00 });
                return;
            }

            if (!ReadHostContextForDevice.ContainsKey(device.Address)) {
                var toTransfer = ServiceLocator.Instance.Get<IMeasurementService>().GetMeasurementsForDataTransfer();
                var innerContext = new ReadHostContext(toTransfer);
                ReadHostContextForDevice.Add(device.Address, innerContext);

                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, innerContext.Header);
                return;
            }

            var context = ReadHostContextForDevice[device.Address];
            var mtu = worker.DevicesMtu.ContainsKey(device.Address) ? worker.DevicesMtu[device.Address] : 20;
            var bytesToSend = context.GetNextBytes(mtu);

            if (context.Complete) {
                DidSendPass.Remove(device.Address);
            }

            if (!bytesToSend.Any()) {
                bytesToSend = new byte[] { 0x00, 0x00, 0x00 };
            }

            worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, bytesToSend);
            return; 
        }

        public async Task HandleWriteAsync(BluetoothDevice device, int requestId, bool preparedWrite, bool responseNeeded, int offset, byte[] value, BluetoothWorker worker)
        {
            if (value.Count() == 3 && value.All(x => x == 0x00)) {
                WriteReceiverContextForDevice.Remove(device.Address);
                if (responseNeeded)
                {
                    worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, value);
                }
                return;
            }
            var isInitialMessage = false;
            if (!WriteReceiverContextForDevice.ContainsKey(device.Address)) {
                isInitialMessage = true;
                WriteReceiverContextForDevice.Add(device.Address, new WriteReceiverContext(value));
            }
            var context = WriteReceiverContextForDevice[device.Address];
            var messageName = WriteReceiverContextForDevice[device.Address].MessageName;
            if (messageName != DataExchangeFirstMessage.MessageNameConstant && messageName != DataExchangeConfirmationMessage.MessageNameConstant) {
                System.Diagnostics.Debug.WriteLine($"Device {device.Address} wrote something which I don't know how to handle to data characteristic!");
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, Encoding.UTF8.GetBytes("wrong pass phrase!"));
                WriteReceiverContextForDevice.Remove(device.Address);
                return;
            }

            if (!isInitialMessage) {
                context.AddMessagePart(value);
            }

            if (!context.ReadComplete) {
                if (responseNeeded)
                {
                    worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, value);
                }
                return;
            } else {
                //lets be open for new write requests
                WriteReceiverContextForDevice.Remove(device.Address);
            }


            var messageRaw = context.GetMessageAsJson();
            var message = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseBluetoothMessage>(messageRaw);


            //initiate auth process
            if (message.MessageName == DataExchangeFirstMessage.MessageNameConstant)
            {
                System.Diagnostics.Debug.WriteLine($"Device {device.Address} authenticated with passphrase");
                if (!DidSendPass.ContainsKey(device.Address))
                {
                    DidSendPass.Add(device.Address, true);
                }
                ResetStateForDevice(device.Address);
            }

            if (message.MessageName == DataExchangeConfirmationMessage.MessageNameConstant && ReadHostContextForDevice.ContainsKey(device.Address)) {
                var messageToDelete = ReadHostContextForDevice[device.Address].Message as DataExchangeMessage;
                ServiceLocator.Instance.Get<IMeasurementService>().DeleteSurveyMeasurement(messageToDelete);
                ResetStateForDevice(device.Address);
                if (DidSendPass.ContainsKey(device.Address))
                {
                    DidSendPass.Remove(device.Address);
                }
            }

            if (responseNeeded) {
                worker.GattServer.SendResponse(device, requestId, Android.Bluetooth.GattStatus.Success, offset, value);
            }
        }

        private void ResetStateForDevice(string address) {

            if (ReadHostContextForDevice.ContainsKey(address)) {
                ReadHostContextForDevice.Remove(address);
            }
        }
    }
}
