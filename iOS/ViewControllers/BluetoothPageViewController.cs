using Foundation;
using System;
using UIKit;
using Happimeter.Views;
using Xamarin.Forms;
using Happimeter.Core.Database;
using Happimeter.ViewModels.Forms;
using System.Linq;

namespace Happimeter.iOS
{
    public partial class BluetoothPageViewController : UINavigationController
    {
        private BluetoothMainPage _bluetoothMainPage;
        private BluetoothPairingPage _bluetoothPairingPage;

        public BluetoothPageViewController (IntPtr handle) : base (handle)
        {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            if (context.Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive) != null) {
                SetBluetoothMainPage();
            } else {
                SetBluetoothPairingPage();
            }
        }

        private void SetBluetoothMainPage() {
            if (_bluetoothPairingPage != null && (_bluetoothPairingPage.BindingContext as BluetoothPairingPageViewModel) != null)
            {
                (_bluetoothPairingPage.BindingContext as BluetoothPairingPageViewModel).OnPairedDevice -= ChangeToBluetoothMainPage;
            }

            _bluetoothMainPage = new BluetoothMainPage();
            ((BluetoothMainPageViewModel)_bluetoothMainPage.BindingContext).OnRemovedPairing += ChangeToBluetoothPairingPage;
            var startSurveyVc = _bluetoothMainPage.CreateViewController();
            PushViewController(startSurveyVc, true);
            startSurveyVc.Title = _bluetoothMainPage.Title;   
        }

        private void SetBluetoothPairingPage() {
            if (_bluetoothMainPage != null && (_bluetoothMainPage.BindingContext as BluetoothMainPageViewModel) != null)
            {
                (_bluetoothMainPage.BindingContext as BluetoothMainPageViewModel).OnRemovedPairing -= ChangeToBluetoothPairingPage;
            }

            _bluetoothPairingPage = new BluetoothPairingPage();
            ((BluetoothPairingPageViewModel)_bluetoothPairingPage.BindingContext).OnPairedDevice += ChangeToBluetoothMainPage;
            var startSurveyVc = _bluetoothPairingPage.CreateViewController();
            PushViewController(startSurveyVc, true);
            startSurveyVc.Title = _bluetoothPairingPage.Title;    
        }

        private void ChangeToBluetoothPairingPage(object sender, EventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                var viewControllers = ViewControllers.ToList();
                viewControllers.Clear();
                ViewControllers = viewControllers.ToArray();
                SetBluetoothPairingPage();
            });
        }

        private void ChangeToBluetoothMainPage(object sender, EventArgs e)
        {
            InvokeOnMainThread(() => {
                var viewControllers = ViewControllers.ToList();
                viewControllers.Clear();
                ViewControllers = viewControllers.ToArray();
                SetBluetoothMainPage();    
            });
        }
    }
}