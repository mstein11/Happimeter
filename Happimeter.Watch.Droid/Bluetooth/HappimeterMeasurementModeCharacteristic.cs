using System;
using Android.Bluetooth;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
    public class HappimeterMeasurementModeCharacteristic : BluetoothGattCharacteristic
    {
        private const GattProperty GattProperties = GattProperty.Write;
        private const GattPermission GattPermissions = GattPermission.Write;


        public HappimeterMeasurementModeCharacteristic() : base(uuid: UUID.FromString(UuidHelper.MeasurementModeCharacteristicUuidString), properties: GattProperties, permissions: GattPermissions)
        {
        }

        public void HandleWriteJson(string json, string deviceAddress)
        {
            var message = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseBluetoothMessage>(json);


            //initiate auth process
            if (message.MessageName == SwitchMeasurementModeMessage.MessageNameConstant)
            {
                System.Diagnostics.Debug.WriteLine($"Device {deviceAddress} sent measurementMode message");

                var mesVal = message.MessageValue;
                if (mesVal == null)
                {
                    //message value of null indicates that we want to run in Continous Mode
                    ServiceLocator.Instance.Get<IDeviceService>().SetContinousMeasurementMode();
                }
                else
                {
                    int duration;
                    if (int.TryParse(mesVal, out duration))
                    {
                        //if we can parse it, we have the measurement interval in seconds
                        ServiceLocator.Instance.Get<IDeviceService>().SetBatterySaferMeasurementMode(duration);
                    }
                    else
                    {
                        //if we can not parse it, we go with the default
                        ServiceLocator.Instance.Get<IDeviceService>().SetBatterySaferMeasurementMode();
                    }
                }
            }
        }
    }
}
