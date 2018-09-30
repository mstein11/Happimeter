
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Happimeter.Droid.Activities;
using Happimeter.Droid.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Plugin.Threading;
using Happimeter.Views;
using System.Threading;
using Happimeter.Views.MoodOverview;
using Happimeter.ViewModels.Forms;
using Happimeter.Core.Helper;
using Happimeter.Core.Database;

namespace Happimeter.Droid.Fragments
{
    public class FragmentContainer : Android.Support.V4.App.Fragment, OnBackPressListener
    {
        public static FragmentContainer NewInstance(int page)
        {
            var container = new FragmentContainer();
            var bundle = new Bundle(1);
            bundle.PutInt("TabIndex", page);
            container.Arguments = bundle;
            return container;
        }

        public FragmentContainer()
        {
        }

        public ContentPage ChildPage { get; set; }



        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var pageIndex = Arguments.GetInt("TabIndex");
            if (pageIndex < 0 || pageIndex > 3)
            {
                throw new ArgumentException("Pageindex must be between 0 and 3");
            }
            switch (pageIndex)
            {
                case 0:
                    ChildPage = GetInitializeSurveyView();
                    break;
                case 1:
                    ChildPage = GetSurveyOverviewListPage();
                    break;
                case 2:
                    ChildPage = GetBtPage();
                    break;
                case 3:
                    ChildPage = GetSettingsPage();
                    break;
            }

            // Create your fragment here
        }

        public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            Android.Views.View rootView = inflater.Inflate(Resource.Layout.fragment_container, container, false);
            var transaction = ChildFragmentManager.BeginTransaction();

            var childFragment = ChildPage.CreateSupportFragment(TabMainActivity.Instance);
            transaction.Replace(Resource.Id.fragment_container_content, childFragment);
            transaction.Commit();
            return rootView;
        }

        public bool OnBackPressed()
        {
            return new BackPressImpl(this).OnBackPressed();
        }

        public void PopBackStackToRoot()
        {

            var count = ChildFragmentManager.BackStackEntryCount;
            if (count == 0)
            {
                return;
            }
            var entry = ChildFragmentManager.GetBackStackEntryAt(count - 1);

            if (entry == null)
            {
                return;
            }
            var index = entry.Id;
            ChildFragmentManager.PopBackStack(0, (int)PopBackStackFlags.Inclusive);
        }

        public void TransitionToPage(ContentPage page, bool addToBackStack = false)
        {
            var fragment = page.CreateSupportFragment(TabMainActivity.Instance);
            var transaction = ChildFragmentManager.BeginTransaction();
            if (addToBackStack)
            {
                transaction.AddToBackStack(null);
            }
            transaction.Replace(Resource.Id.fragment_container_content, fragment);
            transaction.CommitAllowingStateLoss();
            ChildPage = page;
        }


        public InitializeSurveyView GetInitializeSurveyView()
        {
            var initSurvey = new InitializeSurveyView();
            //var initSurveyFrag = initSurvey.CreateSupportFragment(TabMainActivity.Instance);
            initSurvey.StartSurveyClickedEvent += (sender, e) =>
            {
                var surveyPage = new SurveyPage();
                EventHandler finishedSurveyHandler = null;
                finishedSurveyHandler = (finSender, finE) =>
                {
                    var finalizeSurveyPage = new FinalizeSurveyPage();
                    this.TransitionToPage(finalizeSurveyPage, true);
                    surveyPage.FinishedSurvey -= finishedSurveyHandler;
                    Timer timer = null;
                    timer = new Timer((obj) =>
                    {
                        //fragmentContainer1.TransitionToPage(initSurvey);
                        this.PopBackStackToRoot();
                        timer.Dispose();
                    }, null, 2000, System.Threading.Timeout.Infinite);
                };
                surveyPage.FinishedSurvey += finishedSurveyHandler;
                this.TransitionToPage(surveyPage, true);
            };
            return initSurvey;
        }

        public SurveyOverviewListPage GetSurveyOverviewListPage()
        {
            var overviewPage = new SurveyOverviewListPage();
            overviewPage.ItemSelectedEvent += (sender, e) =>
            {
                if (sender is SurveyOverviewItemViewModel vm)
                {
                    var detailPage = new SurveyOverviewDetailPage(vm.Date);
                    this.TransitionToPage(detailPage, true);
                }
            };
            return overviewPage;
        }
        public ContentPage GetBtPage()
        {
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

            EventHandler removePairingHandler = null;
            EventHandler addPairingHandler = null;
            removePairingHandler = (sender, e) =>
            {
                var oldBtMainPage = this.ChildPage;
                var btPairingPage = new BluetoothPairingPage();
                this.TransitionToPage(btPairingPage);

                var vm = (btPairingPage.BindingContext as BluetoothPairingPageViewModel);
                vm.OnPairedDevice += addPairingHandler;

                var oldVm = (oldBtMainPage.BindingContext as BluetoothMainPageViewModel);
                oldVm.OnRemovedPairing -= removePairingHandler;
            };
            addPairingHandler = (sender, e) =>
            {
                var oldBtPairingPage = this.ChildPage;
                var btMainPage = new BluetoothMainPage();
                this.TransitionToPage(btMainPage);

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
            return btPage;
        }
        public SettingsPage GetSettingsPage()
        {
            var settingPage = new SettingsPage();
            settingPage.ViewModel.ListMenuItemSelected += (sender, e) =>
            {
                if (sender is ContentPage selectedPage)
                {
                    this.TransitionToPage(selectedPage, true);
                }
            };
            return settingPage;
        }
    }
}
