using System;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.DependencyInjection;
using Happimeter.Interfaces;
using Plugin.BluetoothLE;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Xml.Serialization;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Happimeter
{
    public class App
    {
        public static bool UseMockDataStore = true;
        public static string BackendUrl = "http://localhost:5000";

        public static void Initialize()
        {
            if (UseMockDataStore)
                ServiceLocator.Instance.Register<IDataStore<Item>, MockDataStore>();
            else
                ServiceLocator.Instance.Register<IDataStore<Item>, CloudDataStore>();

            Container.RegisterElements();
            var sharedDb = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var pairing = sharedDb.Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);
            if (pairing != null)
            {
                ServiceLocator.Instance.Get<IBeaconWakeupService>().StartWakeupForBeacon();
            }
        }

        public static void AppResumed()
        {
            BluetoothAlertIfNeeded();
        }

        public static void BluetoothAlertIfNeeded()
        {
            if (CrossBleAdapter.Current.Status != AdapterStatus.PoweredOn)
            {
                if (Application.Current != null && Application.Current.MainPage != null)
                {
                    Application.Current.MainPage.DisplayAlert("Bluetooth deactivated", "Please enable Bluetooth", "Ok");
                }
            }
        }
    }
}
