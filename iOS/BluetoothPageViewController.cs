using Foundation;
using System;
using UIKit;
using Happimeter.Views;
using Xamarin.Forms;
using Happimeter.Core.Database;

namespace Happimeter.iOS
{
    public partial class BluetoothPageViewController : UINavigationController
    {
        public BluetoothPageViewController (IntPtr handle) : base (handle)
        {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            if (context.Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive) != null) {
                var formsPage = new BluetoothMainPage();
                var startSurveyVc = formsPage.CreateViewController();
                PushViewController(startSurveyVc, true);
                startSurveyVc.Title = formsPage.Title;    
            } else {
                var formsPage = new BluetoothPairingPage();
                var startSurveyVc = formsPage.CreateViewController();
                PushViewController(startSurveyVc, true);
                startSurveyVc.Title = formsPage.Title;    
            }
        }
    }
}