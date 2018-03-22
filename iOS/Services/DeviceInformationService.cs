using System;
using Happimeter.Interfaces;
using UIKit;

namespace Happimeter.iOS.Services
{
    public class DeviceInformationService : IDeviceInformationService
    {
        public DeviceInformationService()
        {
        }

        public string GetPhoneOs() {
            return "iOS";
        }

        public string GetDeviceName() {
            return UIDevice.CurrentDevice.Name;
        } 
    }
}
