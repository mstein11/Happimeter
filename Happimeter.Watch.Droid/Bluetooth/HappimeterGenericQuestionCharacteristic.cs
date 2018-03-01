using System;
using Android.Bluetooth;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
    public class HappimeterGenericQuestionCharacteristic : BluetoothGattCharacteristic
    {
        private const GattProperty GattProperties = GattProperty.Write;
        private const GattPermission GattPermissions = GattPermission.Write;


        public HappimeterGenericQuestionCharacteristic() : base(uuid: UUID.FromString(UuidHelper.GenericQuestionCharacteristicUuidString),properties: GattProperties,permissions: GattPermissions)
        {
        }

        public void HandleWriteJson(string json, string deviceAddress)
        {
            var message = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseBluetoothMessage>(json);


            //initiate auth process
            if (message.MessageName == GenericQuestionMessage.MessageNameConstant)
            {
                System.Diagnostics.Debug.WriteLine($"Device {deviceAddress} Sent generic questions");

                var genericQuestionMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<GenericQuestionMessage>(json);
                ServiceLocator.Instance.Get<IMeasurementService>().AddGenericQuestions(genericQuestionMessage.Questions);
            }
        }
    }
}
