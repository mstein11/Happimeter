using System;
using Happimeter.Interfaces;
using UIKit;
using Happimeter.Views;
using Xamarin.Forms;

namespace Happimeter.iOS.Services
{
    public class NativeNavigationService : INativeNavigationService
    {
        public void NavigateToLoginPage()
        {
            /*
            UIStoryboard board = UIStoryboard.FromName("Main", null);
            UIViewController ctrl = (UIViewController)board.InstantiateViewController("SignInViewController");
            ctrl.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
            UIApplication.SharedApplication.KeyWindow.RootViewController = ctrl;
            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
            */
            var formsPage = new SignInPage();
            var formsPageVc = formsPage.CreateViewController();
            UIApplication.SharedApplication.KeyWindow.RootViewController = formsPageVc;
            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
        }

        public void NavigateToLoggedInPage()
        {
            UIStoryboard board = UIStoryboard.FromName("Main", null);
            UIViewController ctrl = (UIViewController)board.InstantiateViewController("tabViewController");
            ctrl.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
            UIApplication.SharedApplication.KeyWindow.RootViewController = ctrl;
            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
        }
    }
}
