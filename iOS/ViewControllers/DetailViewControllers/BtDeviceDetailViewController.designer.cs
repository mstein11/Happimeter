// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Happimeter.iOS
{
    [Register ("BtDeviceDetailViewController")]
    partial class BtDeviceDetailViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton Connect { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ConnectionStatus { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel Name { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel NameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel Rssi { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel RssiLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton SendNotification { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel Uuid { get; set; }

        [Action ("Connect_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void Connect_TouchUpInside (UIKit.UIButton sender);

        [Action ("UIButton50403_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void UIButton50403_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (Connect != null) {
                Connect.Dispose ();
                Connect = null;
            }

            if (ConnectionStatus != null) {
                ConnectionStatus.Dispose ();
                ConnectionStatus = null;
            }

            if (Name != null) {
                Name.Dispose ();
                Name = null;
            }

            if (NameLabel != null) {
                NameLabel.Dispose ();
                NameLabel = null;
            }

            if (Rssi != null) {
                Rssi.Dispose ();
                Rssi = null;
            }

            if (RssiLabel != null) {
                RssiLabel.Dispose ();
                RssiLabel = null;
            }

            if (SendNotification != null) {
                SendNotification.Dispose ();
                SendNotification = null;
            }

            if (Uuid != null) {
                Uuid.Dispose ();
                Uuid = null;
            }
        }
    }
}