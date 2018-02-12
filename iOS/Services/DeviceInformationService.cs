using System;
using Happimeter.Interfaces;

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
    }
}
