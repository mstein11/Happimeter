using System;
using Happimeter.Droid.Services;
using Happimeter.Interfaces;

namespace Happimeter.Droid.DependencyInjection
{
    public static class Container
    {

        public static void RegisterElements() {
            ServiceLocator.Instance.Register<IBeaconWakeupService, BeaconWakeupService>();
            ServiceLocator.Instance.Register<IDeviceInformationService, DeviceInformationService>();
        }
    }
}
