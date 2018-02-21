using Foundation;
using System;
using UIKit;
using Happimeter.Views.MoodOverview;
using Xamarin.Forms;

namespace Happimeter.iOS
{
    public partial class MoodOverviewViewController : UINavigationController
    {
        public MoodOverviewViewController (IntPtr handle) : base (handle)
        {
            var formsPage = new SurveyOverviewListPage();
            var startSurveyVc = formsPage.CreateViewController();
            PushViewController(startSurveyVc, true);
            startSurveyVc.Title = formsPage.Title;
        }
    }
}