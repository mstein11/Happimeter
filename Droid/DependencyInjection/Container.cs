using System;
using Happimeter.Core.Helper;
using Happimeter.Droid.Services;
using Happimeter.Interfaces;

namespace Happimeter.Droid.DependencyInjection
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
