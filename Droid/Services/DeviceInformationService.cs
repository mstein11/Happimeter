using System;
using Happimeter.Interfaces;

namespace Happimeter.Droid.Services
{
    public class DeviceInformationService : IDeviceInformationService
    {
        public DeviceInformationService()
        {
        }

        public string GetPhoneOs()
        {
            return "Android";
        }

        public string GetDeviceName() {
            return Android.OS.Build.Model;
        } 
    }
}
