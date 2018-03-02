using System;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using Happimeter.Models.ServiceModels;
using Xamarin.Forms;

namespace Happimeter.ViewModels.Forms
{
    public class SignInViewModel : BaseViewModel
    {
        public SignInViewModel()
        {
            LoginButtonIsEnabled = true;
            LoginButtonText = "Login / Signup";
            LoginCommand = new Command(async () =>
            {
                LoginButtonIsEnabled = false;
                LoginButtonText = "Loading...";
                ErrorTextVisible = false;
                var apiService = ServiceLocator.Instance.Get<IHappimeterApiService>();
                var result = await apiService.Auth(UserName, Password);

                if (result.IsSuccess) {
                    //successfully logged in.
                    ServiceLocator.Instance.Get<INativeNavigationService>().NavigateToLoggedInPage();
                } else {
                    //there is no account with given password & username combination, lets try to sign him up
                    var registerResult = await apiService.CreateAccount(UserName, Password);
                    if (registerResult.IsSuccess) {
                        //we successfully signed up. We need to login again to retrieve token and other information.
                        var loginResult = await apiService.Auth(UserName, Password);
                        if (loginResult.IsSuccess) {
                            //successfully logged in.
                            ServiceLocator.Instance.Get<INativeNavigationService>().NavigateToLoggedInPage();
                        } else {
                            //something strange happend
                            ErrorText = "An unknown error occured";
                        }
                    } else if (registerResult.ResultType == RegisterUserResultTypes.ErrorNoInternet) {
                        //no internet
                        ErrorText = "You need an internet connection to login / sign up";
                        ErrorTextVisible = true;
                    } else if (registerResult.ResultType == RegisterUserResultTypes.ErrorInvalidEmail) {
                        ErrorText = "You provided an invalid Email address";
                        ErrorTextVisible = true;
                    } else if (registerResult.ResultType == RegisterUserResultTypes.ErrorPasswordInsufficient) {
                        ErrorText = "Your provided an insufficient password. Passwords need to have at least 5 characters";
                        ErrorTextVisible = true;
                    } else if (registerResult.ResultType == RegisterUserResultTypes.ErrorUserAlreadyTaken) {
                        ErrorText = "You provided the wrong password!";
                        ErrorTextVisible = true;
                    } else if (registerResult.ResultType == RegisterUserResultTypes.ErrorUnknown) {
                        //something strange happened
                        ErrorText = "An unknown error occured";
                        ErrorTextVisible = true;
                    }
                }
                LoginButtonIsEnabled = true;
                LoginButtonText = "Login / Signup";
            });
        }

        private string _userName;
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool _errorTextVisible;
        public bool ErrorTextVisible
        {
            get => _errorTextVisible;
            set => SetProperty(ref _errorTextVisible, value);
        }

        private string _errorText;
        public string ErrorText
        {
            get => _errorText;
            set => SetProperty(ref _errorText, value);
        }

        public Command LoginCommand { get; set; }

        private bool _loginButtonIsEnabled;
        public bool LoginButtonIsEnabled {
            get => _loginButtonIsEnabled;
            set => SetProperty(ref _loginButtonIsEnabled, value);
        }

        private string _loginButtonText;
        public string LoginButtonText
        {
            get => _loginButtonText;
            set => SetProperty(ref _loginButtonText, value);
        }
    }
}

