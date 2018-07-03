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
			if (UseMockDataStore)
				ServiceLocator.Instance.Register<IDataStore<Item>, MockDataStore>();
			else
				ServiceLocator.Instance.Register<IDataStore<Item>, CloudDataStore>();

			Container.RegisterElements();
			await ServerSync();
			var sharedDb = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			var pairing = sharedDb.Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);
			if (pairing != null)
			{
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
				await ServiceLocator.Instance.Get<IPredictionService>().DownloadAndSavePrediction();
				await ServiceLocator.Instance.Get<IProximityService>().DownloadAndSaveProximity();
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

			//dict.Add("notConverter", new NotConverter());

			ResourceDict = dict;
		}
	}
}
