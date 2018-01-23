using Foundation;
using System;
using UIKit;
using Happimeter.iOS.Services;

namespace Happimeter.iOS
{
    public partial class LoginViewController : UIViewController
    {

        private LoginService _loginService;


        async partial void UIButton4921_TouchUpInside(UIButton sender)
        {
            if (Username.Text != null && Password.Text != null)
            {
                var loginResult = await _loginService.Login(Username.Text, Password.Text);
                if (loginResult.IsSuccess) {
                    
                } else {
                    ShowToast("Error while loggin in", View);
                }

            }
        }

        partial void UIButton4408_TouchUpInside(UIButton sender)
        {

        }

        public LoginViewController (IntPtr handle) : base (handle)
        {
            _loginService = new LoginService();
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