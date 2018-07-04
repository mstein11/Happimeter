using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using Happimeter.Core.Services;

namespace Happimeter.Droid
{
	[Activity(Label = "@string/app_name", Theme = "@style/SplashTheme", MainLauncher = true)]
	public class SplashActivity : AppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var isAuth = ServiceLocator.Instance.Get<IAccountStoreService>().IsAuthenticated();
			if (isAuth)
			{
				ServiceLocator.Instance.Get<INativeNavigationService>().NavigateToLoggedInPage();
			}
			else
			{
				ServiceLocator.Instance.Get<INativeNavigationService>().NavigateToLoginPage();
			}
			Finish();
		}
	}
}
