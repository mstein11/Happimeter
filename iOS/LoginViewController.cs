using Foundation;
using System;
using UIKit;
using Happimeter.iOS.Services;
using Happimeter.Interfaces;
using Happimeter.iOS.ViewControllers;
using Happimeter.Core.Helper;

namespace Happimeter.iOS
{
    public partial class LoginViewController : AbstractViewController
    {
        async partial void UIButton17630_TouchUpInside(UIButton sender)
        {
            if (Username.Text != null && Password.Text != null)
            {
                var loginResult = await _loginService.Login(Username.Text, Password.Text);
                if (loginResult.IsSuccess)
                {
                    UIStoryboard board = UIStoryboard.FromName("Main", null);
                    UIViewController ctrl = (UIViewController)board.InstantiateViewController("tabViewController");
                    ctrl.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
                    this.PresentViewController(ctrl, true, null);
                }
                else
                {
                    ShowToast("Error while loggin in", View);
                }

            }
        }
      

        partial void UIButton3278_TouchUpInside(UIButton sender)
        {
            var blService = ServiceLocator.Instance.Get<IBluetoothService>();
            blService.StartScan();
        }

        private LoginService _loginService;


        public override bool HandlesKeyboardNotifications()
        {
            return true;
        }

        /*
        async partial void UIButton4921_TouchUpInside(UIButton sender)
        {
            if (Username.Text != null && Password.Text != null)
            {
                var loginResult = await _loginService.Login(Username.Text, Password.Text);
                if (loginResult.IsSuccess) {
                    UIStoryboard board = UIStoryboard.FromName("MainStoryboard", null);
                    UIViewController ctrl = (UIViewController)board.InstantiateViewController("tabViewController");
                    ctrl.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
                    this.PresentViewController(ctrl, true, null);
                } else {
                    ShowToast("Error while loggin in", View);
                }

            }
        }

        partial void UIButton4408_TouchUpInside(UIButton sender)
        {


        }
*/
        public LoginViewController (IntPtr handle) : base (handle)
        {
            _loginService = new LoginService();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (Username != null && Password != null) {
                Username.ShouldReturn += (UITextField textField) => {
                    textField.ResignFirstResponder();
                    return true;
                };    
                Password.ShouldReturn += (UITextField textField) => {
                    textField.ResignFirstResponder();
                    return true;
                };    
            }

        }

        public void ShowToast(String message, UIView view)
        {
            UIView residualView = view.ViewWithTag(1989);
            if (residualView != null)
                residualView.RemoveFromSuperview();

            var viewBack = new UIView(new CoreGraphics.CGRect(83, 0, 300, 100));
            viewBack.BackgroundColor = UIColor.Black;
            viewBack.Tag = 1989;
            UILabel lblMsg = new UILabel(new CoreGraphics.CGRect(0, 20, 300, 60));
            lblMsg.Lines = 2;
            lblMsg.Text = message;
            lblMsg.TextColor = UIColor.White;
            lblMsg.TextAlignment = UITextAlignment.Center;
            viewBack.Center = view.Center;
            viewBack.AddSubview(lblMsg);
            view.AddSubview(viewBack);
            //roundtheCorner(viewBack);
            UIView.BeginAnimations("Toast");
            UIView.SetAnimationDuration(3.0f);
            viewBack.Alpha = 0.0f;
            UIView.CommitAnimations();
        }
    }
}