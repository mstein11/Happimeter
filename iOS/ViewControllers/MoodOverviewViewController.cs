using Foundation;
using System;
using UIKit;
using Happimeter.Views.MoodOverview;
using Xamarin.Forms;
using Happimeter.ViewModels.Forms;

namespace Happimeter.iOS
{
    public partial class MoodOverviewViewController : UINavigationController
    {
        public MoodOverviewViewController(IntPtr handle) : base(handle)
        {
            var formsPage = new SurveyOverviewListPage();
            var startSurveyVc = formsPage.CreateViewController();
            PushViewController(startSurveyVc, true);
            startSurveyVc.Title = formsPage.Title;

            formsPage.ItemSelectedEvent += (obj, e) =>
            {
                var vm = obj as SurveyOverviewItemViewModel;
                if (vm != null)
                {
                    var detailPage = new SurveyOverviewDetailPage(vm.Date);
                    var detailPagevc = detailPage.CreateViewController();
                    PushViewController(detailPagevc, true);
                    return;
                }
            };
        }
    }
}