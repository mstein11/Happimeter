using System;
using Android.App;
using Android.Content;
using Happimeter.Droid.Activities;
using Happimeter.Interfaces;

namespace Happimeter.Droid.Services
{
    public class NativeNavigationService : INativeNavigationService
    {
        public NativeNavigationService()
        {
        }

        public void NavigateToLoggedInPage()
        {
            var newIntent = new Intent(Application.Context, typeof(TabMainActivity));
            newIntent.AddFlags(ActivityFlags.ClearTop);
            newIntent.AddFlags(ActivityFlags.SingleTop);

            Application.Context.StartActivity(newIntent);
        }

        public void NavigateToLoginPage()
        {
            var newIntent = new Intent(Application.Context, typeof(SignInActivity));
            newIntent.AddFlags(ActivityFlags.ClearTop);
            newIntent.AddFlags(ActivityFlags.SingleTop);

            Application.Context.StartActivity(newIntent);
        }
    }
}
