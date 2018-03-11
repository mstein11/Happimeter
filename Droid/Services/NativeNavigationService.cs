﻿using System;
using Android.App;
using Android.Content;
using Happimeter.Droid.Activities;
using Happimeter.Interfaces;
using Plugin.CurrentActivity;

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

            CrossCurrentActivity.Current.Activity.StartActivity(newIntent);
        }

        public void NavigateToLoginPage()
        {
            var newIntent = new Intent(CrossCurrentActivity.Current.Activity, typeof(SignInActivity));
            newIntent.AddFlags(ActivityFlags.ClearTop);
            newIntent.AddFlags(ActivityFlags.SingleTop);

            CrossCurrentActivity.Current.Activity.StartActivity(newIntent);
        }
    }
}