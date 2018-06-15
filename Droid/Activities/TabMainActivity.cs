
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.Design.Widget;
using Happimeter.Views;
using Xamarin.Forms.Platform.Android;
using Happimeter.Views.MoodOverview;
using Happimeter.Droid.Helpers;
using Happimeter.Core.Helper;
using Happimeter.Core.Database;
using Xamarin.Forms;
using Happimeter.ViewModels.Forms;
using System.Threading;

namespace Happimeter.Droid.Activities
{
	[Activity(Label = "@string/app_name", Icon = "@mipmap/icon",
		LaunchMode = LaunchMode.SingleInstance,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
		ScreenOrientation = ScreenOrientation.Portrait)]
	public class TabMainActivity : BaseActivity
	{

		protected override int LayoutResource => Resource.Layout.tab_main_activity;

		public static TabMainActivity Instance = null;

		ViewPager pager;
		TabsAdapter adapter;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			Instance = this;

			adapter = new TabsAdapter(this, SupportFragmentManager);
			pager = FindViewById<ViewPager>(Resource.Id.viewpager);
			var tabs = FindViewById<TabLayout>(Resource.Id.tabs);
			pager.Adapter = adapter;
			tabs.SetupWithViewPager(pager);
			pager.OffscreenPageLimit = 3;

			pager.PageSelected += (sender, args) =>
			{
				var fragment = adapter.InstantiateItem(pager, args.Position) as IFragmentVisible;

				fragment?.BecameVisible();
			};

			//activate BT tab
			var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
			var hasPairing = context.Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive) != null;
			if (hasPairing)
			{
				pager.CurrentItem = 1;
			}
			else
			{
				pager.CurrentItem = 2;
			}

		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			//MenuInflater.Inflate(Resource.Menu.top_menus, menu);
			return base.OnCreateOptionsMenu(menu);
		}


		public override void OnBackPressed()
		{
			var currentFrag = adapter.FragmentsWithPosition[pager.CurrentItem] as OnBackPressListener;
			if (currentFrag != null)
			{
				var didHandle = currentFrag.OnBackPressed();
				if (!didHandle)
				{
					base.OnBackPressed();
				}
			}
			else
			{
				base.OnBackPressed();
			}
		}
	}

	class TabsAdapter : FragmentStatePagerAdapter
	{

		string[] titles;
		public override int Count => titles.Length;
		public Dictionary<int, Android.Support.V4.App.Fragment> FragmentsWithPosition = new Dictionary<int, Android.Support.V4.App.Fragment>();

		public TabsAdapter(Context context, Android.Support.V4.App.FragmentManager fm) : base(fm)
		{
			titles = context.Resources.GetTextArray(Resource.Array.sections);
		}

		public override Java.Lang.ICharSequence GetPageTitleFormatted(int position) =>
							new Java.Lang.String(titles[position]);

		public override Android.Support.V4.App.Fragment GetItem(int position)
		{
			if (!FragmentsWithPosition.Any())
			{
				var initSurvey = new InitializeSurveyView();
				var overviewPage = new SurveyOverviewListPage();
				ContentPage btPage = null;
				var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
				var hasPairing = context.Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive) != null;

				if (hasPairing)
				{
					btPage = new BluetoothMainPage();
				}
				else
				{
					btPage = new BluetoothPairingPage();
				}

				var fragmentContainer1 = new Fragments.FragmentContainer(initSurvey);

				//var initSurveyFrag = initSurvey.CreateSupportFragment(TabMainActivity.Instance);
				initSurvey.StartSurveyClickedEvent += (sender, e) =>
				{
					var surveyPage = new SurveyPage();
					fragmentContainer1.TransitionToPage(surveyPage, true);
					EventHandler finishedSurveyHandler = null;
					finishedSurveyHandler = (finSender, finE) =>
					{
						var finalizeSurveyPage = new FinalizeSurveyPage();
						fragmentContainer1.TransitionToPage(finalizeSurveyPage, true);
						surveyPage.FinishedSurvey -= finishedSurveyHandler;
						Timer timer = null;
						timer = new Timer((obj) =>
						{
							//fragmentContainer1.TransitionToPage(initSurvey);
							fragmentContainer1.PopBackStackToRoot();
							timer.Dispose();
						}, null, 2000, System.Threading.Timeout.Infinite);
					};
					surveyPage.FinishedSurvey += finishedSurveyHandler;
				};

				var fragmentContainer2 = new Fragments.FragmentContainer(overviewPage);
				overviewPage.ItemSelectedEvent += (sender, e) =>
				{
					var vm = sender as SurveyOverviewItemViewModel;
					if (vm != null)
					{
						var detailPage = new SurveyOverviewDetailPage(vm.Date);
						fragmentContainer2.TransitionToPage(detailPage, true);
					}
				};

				var fragmentContainer3 = new Fragments.FragmentContainer(btPage);


				EventHandler removePairingHandler = null;
				EventHandler addPairingHandler = null;
				removePairingHandler = (sender, e) =>
				{
					var oldBtMainPage = fragmentContainer3.ChildPage;
					var btPairingPage = new BluetoothPairingPage();
					fragmentContainer3.TransitionToPage(btPairingPage);

					var vm = (btPairingPage.BindingContext as BluetoothPairingPageViewModel);
					vm.OnPairedDevice += addPairingHandler;

					var oldVm = (oldBtMainPage.BindingContext as BluetoothMainPageViewModel);
					oldVm.OnRemovedPairing -= removePairingHandler;
				};
				addPairingHandler = (sender, e) =>
				{
					var oldBtPairingPage = fragmentContainer3.ChildPage;
					var btMainPage = new BluetoothMainPage();
					fragmentContainer3.TransitionToPage(btMainPage);

					var vm = (btMainPage.BindingContext as BluetoothMainPageViewModel);
					vm.OnRemovedPairing += removePairingHandler;

					var oldVm = (oldBtPairingPage.BindingContext as BluetoothPairingPageViewModel);
					oldVm.OnPairedDevice -= addPairingHandler;
				};

				if (hasPairing)
				{
					var vm = ((btPage as BluetoothMainPage).BindingContext as BluetoothMainPageViewModel);
					vm.OnRemovedPairing += removePairingHandler;
				}
				else
				{
					var vm = ((btPage as BluetoothPairingPage).BindingContext as BluetoothPairingPageViewModel);
					vm.OnPairedDevice += addPairingHandler;
				}

				var settingPage = new SettingsPage();
				var fragmentContainer4 = new Fragments.FragmentContainer(settingPage);
				settingPage.ViewModel.ListMenuItemSelected += (sender, e) =>
				{
					var selectedPage = sender as ContentPage;
					if (selectedPage != null)
					{
						fragmentContainer4.TransitionToPage(selectedPage, true);
					}
				};

				FragmentsWithPosition.Add(0, fragmentContainer1);
				FragmentsWithPosition.Add(1, fragmentContainer2);
				FragmentsWithPosition.Add(2, fragmentContainer3);
				FragmentsWithPosition.Add(3, fragmentContainer4);
			}

			switch (position)
			{
				case 0: return FragmentsWithPosition[position];
				case 1: return FragmentsWithPosition[position];
				case 2: return FragmentsWithPosition[position];
				case 3: return FragmentsWithPosition[position];
			}
			return null;
		}

		public override int GetItemPosition(Java.Lang.Object frag) => PositionNone;

	}
}
