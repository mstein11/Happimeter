using Foundation;
using System;
using UIKit;
using Happimeter.iOS.Services;

namespace Happimeter.iOS
{
    public partial class LoginViewController : UIViewController
    {

        private LoginService _loginService;


        partial void UIButton4921_TouchUpInside(UIButton sender)
        {
            if (Username.Text != null && Password.Text != null)
            {
                _loginService.Login(Username.Text, Password.Text);
            }
        }

        partial void UIButton4408_TouchUpInside(UIButton sender)
        {

        }

        public LoginViewController (IntPtr handle) : base (handle)
        {
            _loginService = new LoginService();
        }
    }
}