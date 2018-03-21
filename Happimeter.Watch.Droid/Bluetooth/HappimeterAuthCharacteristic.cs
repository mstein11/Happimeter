using System.Collections.Generic;
using Android.Bluetooth;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Happimeter.Watch.Droid.Workers;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
    public class HappimeterAuthCharacteristic : BluetoothGattCharacteristic
    {
        private const GattProperty GattProperties = GattProperty.Read | GattProperty.Write | GattProperty.Notify;
        private const GattPermission GattPermissions = GattPermission.Read | GattPermission.Write;
        private Dictionary<string, bool> AuthenticationDeviceDidGreat = new Dictionary<string, bool>();

        private Dictionary<string, ReadHostContext> ReadHostContextForDevice = new Dictionary<string, ReadHostContext>();

        public HappimeterAuthCharacteristic() : base(uuid: UUID.FromString(UuidHelper.AuthCharacteristicUuidString), properties: GattProperties, permissions: GattPermissions)
        {
            var uuid = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
            //this.WriteType = GattWriteType.Default;
            var configDescriptor = new BluetoothGattDescriptor(uuid, GattDescriptorPermission.Read | GattDescriptorPermission.Write);
            //AddDescriptor(configDescriptor);
        }


        public ReadHostContext GetReadHostContext(string address)
        {
            if (ReadHostContextForDevice.ContainsKey(address))
            {
                //already has a readhostcontext, just go on
                return ReadHostContextForDevice[address];
            }
            if(!AuthenticationDeviceDidGreat.ContainsKey(address)) {
                System.Diagnostics.Debug.WriteLine($"Device {address} read from auth characteristic without greeting first!");
                return null;
            }

            ReadHostContextForDevice.Add(address, new ReadHostContext(new AuthFirstMessage()));
            return ReadHostContextForDevice[address];
        }

        public void DoneReading(string address)
        {
            AuthenticationDeviceDidGreat.Remove(address);
            ReadHostContextForDevice.Remove(address);
        }

        public void HandleWriteJson(string json, string address)
        {
            var message = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseBluetoothMessage>(json);

            if (message.MessageName != AuthFirstMessage.MessageNameConstant && message.MessageName != AuthSecondMessage.MessageNameConstant)
            {
                System.Diagnostics.Debug.WriteLine($"Device {address} wrote something which I don't know how to handle to auth characteristic!");
                return;
            }

            //initiate auth process
            if (message.MessageName == AuthFirstMessage.MessageNameConstant) {
                System.Diagnostics.Debug.WriteLine($"Device {address} started authentication procedure");
                if (!AuthenticationDeviceDidGreat.ContainsKey(address))
                {
                    AuthenticationDeviceDidGreat.Add(address, true);
                }

                var timer = new System.Threading.Timer((obj) =>
                {
                    BluetoothWorker.GetInstance().SendNotifiation(null, new AuthNotificationMessage(true));
                }, null, 5000, System.Threading.Timeout.Infinite);

                return;
            }

            //finalize auth process
            if (message.MessageName == AuthSecondMessage.MessageNameConstant)
            {
                var messageData = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthSecondMessage>(json);
                System.Diagnostics.Debug.WriteLine($"Device {address} finalized authentication procedure");

                ServiceLocator.Instance.Get<IDeviceService>().AddPairing(messageData);
                return;
            }
        }

    }
}
