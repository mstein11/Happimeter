using System;
using Happimeter.Core.Database;
using Happimeter.Interfaces;
using Happimeter.Core.Helper;
using System.Reactive.Subjects;
using Xamarin.Forms;
namespace Happimeter.ViewModels.Forms.Teams
{
    public class TeamListItemViewModel : BaseViewModel
    {
        private readonly TeamEntry teamObj;
        private ITeamService _teamService;
        public TeamListItemViewModel(TeamEntry team)
        {
            teamObj = team;
            _teamService = ServiceLocator.Instance.Get<ITeamService>();

            Id = team.Id;
            Name = team.Name;
            IsAdmin = team.IsAdmin;
            LeaveDeleteButtonText = IsAdmin ? "Delete Team" : "Leave Team";
            LeaveTeamCommand = new Command(async () =>
            {
                var result = await Application
                    .Current
                    .MainPage
                    .DisplayAlert("Confirm",
                                  $"Are you sure that you want to " +
                                  $"{ (IsAdmin ? "delete" : "leave")} this team?",
                                  IsAdmin ? "Delete Team" : "Leave Team", "Cancel");
                if (!result)
                {
                    return;
                }

                LeaveDeleteButtonText = "Loading...";
                var success = await _teamService.LeaveTeam(Id);
                LeaveDeleteButtonText = IsAdmin ? "Delete Team" : "Leave Team";
                if (!success)
                {
                    await Application
                        .Current
                        .MainPage
                        .DisplayAlert("Error",
                                      "There was an error leaving the team. " +
                                      "Either you have no internet connection " +
                                      "or you already left the team.", "ok");
                    return;

                }
                TeamLeftSubject.OnNext(Id);
            });
        }

        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private bool _idAdmin;
        public bool IsAdmin
        {
            get => _idAdmin;
            set => SetProperty(ref _idAdmin, value);
        }

        private bool _isLeaving;
        public bool IsLeaving
        {
            get => _isLeaving;
            set => SetProperty(ref _isLeaving, value);
        }

        private string _leaveDeleteButtonText;
        public string LeaveDeleteButtonText
        {
            get => _leaveDeleteButtonText;
            set => SetProperty(ref _leaveDeleteButtonText, value);
        }

        public Command LeaveTeamCommand { get; set; }
        private readonly Subject<int> TeamLeftSubject = new Subject<int>();
        public IObservable<int> WhenTeamLeft()
        {
            return TeamLeftSubject;
        }
    }
}
