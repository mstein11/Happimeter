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
    [Register ("AboutViewController")]
    partial class AboutViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView AboutImageView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel DeviceNameValue { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel IsPairedValue { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel LastDataExchangeValue { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel PairedAtValue { get; set; }

        [Action ("UIButton63292_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void UIButton63292_TouchUpInside (UIKit.UIButton sender);

        [Action ("UIButton67110_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void UIButton67110_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (AboutImageView != null) {
                AboutImageView.Dispose ();
                AboutImageView = null;
            }

            if (DeviceNameValue != null) {
                DeviceNameValue.Dispose ();
                DeviceNameValue = null;
            }

            if (IsPairedValue != null) {
                IsPairedValue.Dispose ();
                IsPairedValue = null;
            }

            if (LastDataExchangeValue != null) {
                LastDataExchangeValue.Dispose ();
                LastDataExchangeValue = null;
            }

            if (PairedAtValue != null) {
                PairedAtValue.Dispose ();
                PairedAtValue = null;
            }
        }
    }
}