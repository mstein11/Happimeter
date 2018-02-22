using Foundation;
using System;
using UIKit;
using Happimeter.Views;
using Xamarin.Forms;

namespace Happimeter.iOS
{
    public partial class SettingsPageViewController : UINavigationController
    {
        public SettingsPageViewController (IntPtr handle) : base (handle)
        {
            var formsPage = new SettingsPage();
            var startSurveyVc = formsPage.CreateViewController();
            PushViewController(startSurveyVc, true);
            startSurveyVc.Title = formsPage.Title;
        }
    }
}