using System;
using Happimeter.Interfaces;
using UIKit;

namespace Happimeter.iOS.Services
{
    public class NativeNavigationService : INativeNavigationService
    {
        public void NavigateToLoginPage() {
            UIStoryboard board = UIStoryboard.FromName("Main", null);
            UIViewController ctrl = (UIViewController)board.InstantiateViewController("SignInViewController");
            ctrl.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
            UIApplication.SharedApplication.KeyWindow.RootViewController = ctrl;
            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
        }

        public void NavigateToLoggedInPage() {
            UIStoryboard board = UIStoryboard.FromName("Main", null);
            UIViewController ctrl = (UIViewController)board.InstantiateViewController("tabViewController");
            ctrl.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
            UIApplication.SharedApplication.KeyWindow.RootViewController = ctrl;
            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
        }
    }
}
