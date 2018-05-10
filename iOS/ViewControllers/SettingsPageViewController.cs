using Foundation;
using System;
using UIKit;
using Happimeter.Views;
using Xamarin.Forms;

namespace Happimeter.iOS
{
	public partial class SettingsPageViewController : UINavigationController
	{
		public SettingsPageViewController(IntPtr handle) : base(handle)
		{
			var formsPage = new SettingsPage();
			formsPage.ViewModel.ListMenuItemSelected += (sender, e) =>
			{
				var selectedPage = sender as ContentPage;
				if (selectedPage != null)
				{
					var selectedPageVc = selectedPage.CreateViewController();
					PushViewController(selectedPageVc, true);
					selectedPageVc.Title = selectedPage.Title;
				}
			};
			var startSurveyVc = formsPage.CreateViewController();
			PushViewController(startSurveyVc, true);
			startSurveyVc.Title = formsPage.Title;
			NavigationBar.TintColor = UIColor.White;
		}
	}
}