using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.DependencyInjection;
using Happimeter.Interfaces;
using Plugin.BluetoothLE;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Happimeter.Controls;
using System.Threading.Tasks;
using Happimeter.Views.Converters;
using Happimeter.Services;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Happimeter.Core.Services;
using Happimeter.Converter;
using SuaveControls.Views;
using Plugin.FirebasePushNotification;
using System.Diagnostics;

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

        public static async void Initialize()
        {
            Container.RegisterElements();
            SetupNotification();
            await ServerSync();
            var sharedDb = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var pairing = sharedDb.Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);
            if (pairing != null)
            {
                ServiceLocator.Instance.Get<IBluetoothService>().Init();
                ServiceLocator.Instance.Get<IBeaconWakeupService>().StartWakeupForBeacon();
            }
        }

        public static async void AppResumed()
        {
            BluetoothAlertIfNeeded();
            await ServerSync();

            var sharedDb = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var pairing = sharedDb.Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);
            if (pairing != null)
            {
                ServiceLocator.Instance.Get<IBluetoothService>().Init();
            }
        }

        public static async void BluetoothAlertIfNeeded()
        {
            if (CrossBleAdapter.Current.Status != AdapterStatus.PoweredOn)
            {
                if (Application.Current != null && Application.Current.MainPage != null)
                {
                    Application.Current.MainPage.DisplayAlert("Bluetooth deactivated", "Please enable Bluetooth", "Ok");
                }
            }
        }

        public static async Task ServerSync()
        {
            if (ServiceLocator.Instance.Get<IAccountStoreService>().IsAuthenticated())
            {
                await ServiceLocator.Instance.Get<ISynchronizationService>().Sync();
            }
        }

        private static void SetupNotification()
        {
            ServiceLocator.Instance.Get<INotificationService>().SetupNotificationHooks();
        }

        private static void InitResourceDict()
        {
            var dict = new ResourceDictionary();

            dict.Add("Checkbox", new Style(typeof(Checkbox))
            {
                Setters = {
                    new Setter {Property = Checkbox.OutlineColorProperty, Value = Color.Black},
                    new Setter {Property = Checkbox.CheckedOutlineColorProperty, Value = Color.Black},
                    new Setter {Property = Checkbox.CheckColorProperty, Value = Color.FromHex("#b71c1c")}
                }
            });

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
                    new Setter {Property = Button.TextColorProperty, Value = Color.White},
                }
            });

            dict.Add("TextOnBackground", new Style(typeof(Label))
            {
                Setters = {
                    new Setter {Property = Label.FontSizeProperty, Value = 14},
                    new Setter {Property = Label.TextColorProperty, Value = Color.White},
                    new Setter {Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center},
                },
            });

            dict.Add("HeaderOnBackground", new Style(typeof(Label))
            {
                Setters = {
                    new Setter {Property = Label.FontSizeProperty, Value = 24},
                    new Setter {Property = Label.TextColorProperty, Value = Color.White},
                    new Setter {Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center},
                },
            });

            dict.Add("HintTextStyle", new Style(typeof(Label))
            {
                Setters = {
                    new Setter {Property = Label.FontSizeProperty, Value = Xamarin.Forms.Device.GetNamedSize(NamedSize.Small, typeof(Label))},
                    new Setter {Property = Label.TextColorProperty, Value = Color.DimGray},
                },
            });

            dict.Add("FloatingActionButtonStyle", new Style(typeof(FloatingActionButton))
            {
                Setters = {
                    new Setter {Property = Button.PaddingProperty, Value = 10},
                    new Setter {Property = View.HorizontalOptionsProperty, Value = LayoutOptions.Center},
                    new Setter {Property = View.VerticalOptionsProperty, Value = LayoutOptions.CenterAndExpand},
                    new Setter {Property = FloatingActionButton.ButtonColorProperty, Value = Color.FromHex("#b71c1c")}
                },
            });

            dict.Add("anyConverter", new AnyValueConverter());
            dict.Add("notAnyConverter", new NotAnyValueConverter());
            //dict.Add("notConverter", new NotConverter());

            ResourceDict = dict;
        }
    }
}
