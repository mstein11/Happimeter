using Foundation;
using System;
using UIKit;
using Happimeter.Views;
using Xamarin.Forms;

namespace Happimeter.iOS
{
    public partial class SurveyViewController : UINavigationController
    {
        public SurveyViewController (IntPtr handle) : base (handle)
        {
            var formsPage = new InitializeSurveyView();
            var startSurveyVc = formsPage.CreateViewController();
            PushViewController(startSurveyVc, true);
            startSurveyVc.Title = formsPage.Title;
            /*AddChildViewController(startSurveyVc);
            View.Add(startSurveyVc.View);
            startSurveyVc.DidMoveToParentViewController(this);
            EdgesForExtendedLayout = UIRectEdge.None;*/

        }
    }
}