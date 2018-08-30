using System;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Core.Services;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.ServicesBusinessLogic;

namespace Happimeter.Watch.Droid.DependencyInjection
{
	public static class Container
	{
		public static void RegisterElements()
		{
			ServiceLocator.Instance.Register<IDatabaseContext, DatabaseContext>();
			var dbContext = ServiceLocator.Instance.Get<IDatabaseContext>();
			ServiceLocator.Instance.RegisterWithInstance<ISharedDatabaseContext, DatabaseContext>(dbContext as DatabaseContext);
			ServiceLocator.Instance.Register<IMeasurementService, MeasurementService>();
			ServiceLocator.Instance.Register<IConfigService, ConfigService>();
			ServiceLocator.Instance.Register<ILoggingService, LoggingService>();
			ServiceLocator.Instance.Register<IAccountStoreService, AccountStoreService>();
			ServiceLocator.Instance.Register<IDeviceService, DeviceService>();
            ServiceLocator.Instance.Register<IAudioFeaturesService, AudioFeaturesService>();
		}
	}
}
