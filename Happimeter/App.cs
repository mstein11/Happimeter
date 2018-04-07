using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.DependencyInjection;
using Happimeter.Interfaces;
using Plugin.BluetoothLE;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Happimeter.Controls;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Happimeter
{
    public class App
    {
        public static bool UseMockDataStore = true;
        public static string BackendUrl = "http://localhost:5000";

        private static ResourceDictionary _resourceDict;
        public static ResourceDictionary ResourceDict
        {
            get
            {
                if (_resourceDict == null)
                {
                    InitResourceDict();
                }
                return _resourceDict;
            }
            set
            {
                _resourceDict = value;
            }
        }

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

        private static void InitResourceDict()
        {
            var dict = new ResourceDictionary();


            dict.Add("ButtonWithBackground", new Style(typeof(MyButton))
            {
                Setters = {
                    new Setter {Property = VisualElement.BackgroundColorProperty, Value = Color.FromHex("#b71c1c")},
                    new Setter {Property = MyButton.DisabledColorProperty, Value = Color.FromHex("#A9A9A9")},
                    new Setter {Property = Button.TextColorProperty, Value = Color.White}
                }
            });

            dict.Add("ButtonWithoutBackground", new Style(typeof(MyButton))
            {
                Setters = {
                    new Setter {Property = VisualElement.BackgroundColorProperty, Value = Color.Transparent},
                }
            });

            ResourceDict = dict;
        }
    }
}
