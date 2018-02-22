using System;
using Happimeter.Interfaces;
using Happimeter.iOS.Services;

namespace Happimeter.iOS.DependencyInjection
{
    public static class Container
    {
        public static void RegisterElements()
        {
            ServiceLocator.Instance.Register<IBeaconWakeupService, BeaconWakeupService>();
            ServiceLocator.Instance.Register<IDeviceInformationService, DeviceInformationService>();
            ServiceLocator.Instance.Register<INativeNavigationService, NativeNavigationService>();
        }
    }
}
