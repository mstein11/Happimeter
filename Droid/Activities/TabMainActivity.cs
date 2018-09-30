
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
using Happimeter.Droid.Fragments;

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
            //Keyboard overlays content, like in iOS
            Window.SetSoftInputMode(SoftInput.AdjustPan);
            Xamarin.FormsGoogleMaps.Init(this, savedInstanceState);
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
                FragmentsWithPosition.Add(0, Fragments.FragmentContainer.NewInstance(0));
                FragmentsWithPosition.Add(1, Fragments.FragmentContainer.NewInstance(1));
                FragmentsWithPosition.Add(2, Fragments.FragmentContainer.NewInstance(2));
                FragmentsWithPosition.Add(3, Fragments.FragmentContainer.NewInstance(3));
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
