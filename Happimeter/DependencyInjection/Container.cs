using System;
using Happimeter.Interfaces;
using Happimeter.iOS.Services;
using Happimeter.Services;

namespace Happimeter.DependencyInjection
{
    public class Container
    {

        public static void RegisterElements() {
            ServiceLocator.Instance.Register<IRestService, RestService>();
            ServiceLocator.Instance.Register<IHappimeterApiService, HappimeterApiService>();
            ServiceLocator.Instance.Register<IAccountStoreService, AccountStoreService>();
            ServiceLocator.Instance.Register<IBluetoothService, BluetoothService>();
            ServiceLocator.Instance.Register<IBeaconWakeupService, BeaconWakeupService>();
        }
    }
}
