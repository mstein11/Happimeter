using Foundation;
using System;
using UIKit;
using Happimeter.Views;
using Xamarin.Forms;

namespace Happimeter.iOS
{
    public partial class BluetoothPageViewController : UINavigationController
    {
        public BluetoothPageViewController (IntPtr handle) : base (handle)
        {
            var formsPage = new BluetoothMainPage();
            var startSurveyVc = formsPage.CreateViewController();
            PushViewController(startSurveyVc, true);
            startSurveyVc.Title = formsPage.Title;
        }
    }
}