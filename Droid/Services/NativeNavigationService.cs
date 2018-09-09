using System;
using Android.App;
using Android.Content;
using Happimeter.Droid.Activities;
using Happimeter.Interfaces;
using Plugin.CurrentActivity;
using Android.OS;
using System.Diagnostics;

namespace Happimeter.Droid.Services
{
    public class NativeNavigationService : INativeNavigationService
    {
        public NativeNavigationService()
        {
        }

        public void NavigateToLoggedInPage()
        {
            var newIntent = new Intent(CrossCurrentActivity.Current.Activity, typeof(TabMainActivity));
            newIntent.AddFlags(ActivityFlags.ClearTop);
            newIntent.AddFlags(ActivityFlags.SingleTop);
            var oldActivity = CrossCurrentActivity.Current.Activity;
            oldActivity.Finish();
            CrossCurrentActivity.Current.Activity.StartActivity(newIntent);
        }

        public void NavigateToLoginPage()
        {
            var newIntent = new Intent(CrossCurrentActivity.Current.Activity, typeof(SignInActivity));
            newIntent.AddFlags(ActivityFlags.ClearTop);
            newIntent.AddFlags(ActivityFlags.SingleTop);
            var oldActivity = CrossCurrentActivity.Current.Activity;
            oldActivity.Finish();
            CrossCurrentActivity.Current.Activity.StartActivity(newIntent);
        }
    }
}
