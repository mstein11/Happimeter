using Foundation;
using System;
using UIKit;
using Happimeter.Models;
using Happimeter.Interfaces;

namespace Happimeter.iOS
{
    public partial class BtDeviceDetailViewController : UIViewController
    {
        partial void UIButton50403_TouchUpInside(UIButton sender)
        {
            ViewModel.SendNotification();
        }

        partial void Connect_TouchUpInside(UIButton sender)
        {
            var _btService = ServiceLocator.Instance.Get<IBluetoothService>();
            _btService.PairDevice(ViewModel);
            //ViewModel.Connect();
            ViewModel.WhenDeviceReady().Subscribe(x => {
                InvokeOnMainThread(() => {
                    SendNotification.Hidden = false;
                });
            });
        }


        public BluetoothDevice ViewModel { get; set; }

        public BtDeviceDetailViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewModel.OnStatusChanged().Subscribe(status => {
                InvokeOnMainThread(() => {
                    ConnectionStatus.Text = status.ToString();
                });
            });

            ViewModel.Device.WhenRssiUpdated().Subscribe(rssi => {
                InvokeOnMainThread(() => {
                    Rssi.Text = rssi.ToString();
                });
            });

            Name.Text = ViewModel.Device.Name;
            Uuid.Text = ViewModel.Device.Uuid.ToString();
            ConnectionStatus.Text = ViewModel.Device.Status.ToString();
        }
    }
}