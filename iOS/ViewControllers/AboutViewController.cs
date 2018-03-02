using System;
using CoreLocation;
using Foundation;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using UIKit;

namespace Happimeter.iOS
{
    public partial class AboutViewController : UIViewController
    {
        partial void UIButton67110_TouchUpInside(UIButton sender)
        {
            ServiceLocator.Instance.Get<IAccountStoreService>().DeleteAccount();
        }

        partial void UIButton63292_TouchUpInside(UIButton sender)
        {
            Console.WriteLine("ABout to start data exchange");
            ServiceLocator.Instance.Get<IBluetoothService>().ExchangeData();
        }


        public AboutViewModel ViewModel { get; set; }
        public AboutViewController(IntPtr handle) : base(handle)
        {

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();


            ViewModel = new AboutViewModel();
            Title = ViewModel.Title;
            UpdateValuesInView();
            ViewModel.ModelChanged += (sender, e) => {
                UpdateValuesInView();
            };
            var tmp = ServiceLocator.Instance.Get<ISharedDatabaseContext>().GetAllWithChildren<SensorMeasurement>();
            foreach (var measure in tmp) {
                Console.WriteLine(measure.Timestamp);
            }


            ServiceLocator.Instance.Get<IBeaconWakeupService>().StartWakeupForBeacon();
        }

        private void UpdateValuesInView() {
            InvokeOnMainThread(() =>
            {
                IsPairedValue.Text = ViewModel.IsPaired;
                DeviceNameValue.Text = ViewModel.DeviceName;
                LastDataExchangeValue.Text = ViewModel.LastDataExchange;
                PairedAtValue.Text = ViewModel.PairedAt;

            });
        }
    }
}
