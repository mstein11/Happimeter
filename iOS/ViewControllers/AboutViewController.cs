using System;
using CoreLocation;
using Foundation;
using Happimeter.Interfaces;
using UIKit;

namespace Happimeter.iOS
{
    public partial class AboutViewController : UIViewController
    {
        partial void UIButton58642_TouchUpInside(UIButton sender)
        {
            Console.WriteLine("ABout to start data exchange");
            ServiceLocator.Instance.Get<IBluetoothService>().ExchangeData();
        }

        public AboutViewModel ViewModel { get; set; }
        public AboutViewController(IntPtr handle) : base(handle)
        {
            ViewModel = new AboutViewModel();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = ViewModel.Title;

            AppNameLabel.Text = "Happimeter";
            VersionLabel.Text = "1.0";
            AboutTextView.Text = "This app is written in C# and native APIs using the Xamarin Platform. It shares code with its iOS, Android, & Windows versions.";


            ServiceLocator.Instance.Get<IBeaconWakeupService>().StartWakeupForBeacon("F0000000-0000-1000-8000-00805F9B34FB", 0, 1);
        }

        partial void ReadMoreButton_TouchUpInside(UIButton sender) => ViewModel.OpenWebCommand.Execute(null);
    }
}
