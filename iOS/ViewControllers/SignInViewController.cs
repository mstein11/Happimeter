using Foundation;
using System;
using UIKit;
using Happimeter.Views;
using Xamarin.Forms;

namespace Happimeter.iOS
{
    public partial class SignInViewController : UINavigationController
    {
        public SignInViewController (IntPtr handle) : base (handle)
        {
            var formsPage = new SignInPage();
            var startSurveyVc = formsPage.CreateViewController();
            PushViewController(startSurveyVc, true);
            startSurveyVc.Title = formsPage.Title;
        }
    }
}