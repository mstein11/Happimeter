using System;
using Happimeter.Interfaces;
using Happimeter.Core.Helper;
using System.Reactive.Subjects;
namespace Happimeter.ViewModels.Forms.Teams
{
    public class JoinTeamViewModel : BaseViewModel
    {
        private readonly ITeamService _teamService;
        public JoinTeamViewModel()
        {
            _teamService = ServiceLocator.Instance.Get<ITeamService>();
            NormalTextIsVisible = true;
            JoinTeamCommand = new Command(async () =>
            {
                IsLoading = true;
                Error = default(string);
                if (Name == null)
                {
                    NormalTextIsVisible = true;
                    Error = "You must provide a Teamname to join a team";
                    return;
                }
                var result = await _teamService.JoinTeam(Name, Password);
                if (result.Item1 == Services.JoinTeamResult.InternetError)
                {
                    NormalTextIsVisible = true;
                    Error = "No internet connection, please try again later";
                    return;
                }
                else if (result.Item1 == Services.JoinTeamResult.WrongTeam)
                {
                    NormalTextIsVisible = true;
                    Error = "The team you tried to join does not exist";
                    return;
                }
                else if (result.Item1 == Services.JoinTeamResult.WrongPassword)
                {
                    NormalTextIsVisible = true;
                    Error = "You provided the wrong password";
                    return;
                }
                else if (result.Item1 == Services.JoinTeamResult.AlreadyMember)
                {
                    NormalTextIsVisible = true;
                    Error = "You are already a member of that team";
                    return;
                }
                else if (result.Item1 == Services.JoinTeamResult.UnknownError)
                {
                    NormalTextIsVisible = true;
                    Error = "An unknown error, please try again later";
                    return;
                }
                SuccessMessageIsVisible = true;
                //todo: reload view
                TeamJoinedSubject.OnNext(result.Item2.Value);
            });
        }

        private Subject<int> TeamJoinedSubject = new Subject<int>();
        public IObservable<int> WhenTeamSuccessfullyJoined()
        {
            return TeamJoinedSubject;
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _error;
        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (value)
                {
                    SuccessMessageIsVisible = false;
                    NormalTextIsVisible = false;
                }
                SetProperty(ref _isLoading, value);
            }

        }

        private bool _normalTextIsVisible;
        public bool NormalTextIsVisible
        {
            get => _normalTextIsVisible;
            set
            {
                if (value)
                {
                    SuccessMessageIsVisible = false;
                    IsLoading = false;
                }
                SetProperty(ref _normalTextIsVisible, value);
            }
        }

        private bool _successMessageIsVisible;
        public bool SuccessMessageIsVisible
        {
            get => _successMessageIsVisible;
            set
            {
                if (value)
                {
                    NormalTextIsVisible = false;
                    IsLoading = false;
                }
                SetProperty(ref _successMessageIsVisible, value);
            }
        }

        public void Reset()
        {
            Name = default(string);
            Password = default(string);
            Error = default(string);
            NormalTextIsVisible = true;
        }

        public Command JoinTeamCommand { get; set; }
    }
}
