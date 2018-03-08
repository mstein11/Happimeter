
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Happimeter.Views;
using Xamarin.Forms.Platform.Android;

namespace Happimeter.Droid.Activities
{
    [Activity(Label = "SignInActivity")]
    public class SignInActivity : BaseActivity
    {
        protected override int LayoutResource => Resource.Layout.activity_sign_in;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var signInPage = new SignInPage();
            var signInPageFrag = signInPage.CreateSupportFragment(this);

            var transaction = SupportFragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.signin_container, signInPageFrag);
            transaction.Commit();

            // Create your application here
        }
    }
}
