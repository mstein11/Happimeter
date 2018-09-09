using System;
using Happimeter.Core.Database;
using Happimeter.Interfaces;
using Happimeter.Core.Helper;
using System.Reactive.Subjects;
using Xamarin.Forms;
using Happimeter.Helpers;

namespace Happimeter.ViewModels.Forms.Teams
{
    public class TeamListItemViewModel : BaseViewModel
    {
        private TeamEntry teamObj;
        private ITeamService _teamService;
        public TeamListItemViewModel(TeamEntry team)
        {
            Update(team);
            _teamService = ServiceLocator.Instance.Get<ITeamService>();

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

        public void Update(TeamEntry team)
        {
            teamObj = team;
            Id = team.Id;
            Name = team.Name;
            IsAdmin = team.IsAdmin;
            MoodIconImagePath = UiHelper.GetImagePathForMood(teamObj.Activation, teamObj.Pleasance);
            MoodString = teamObj.Activation == null
                                || teamObj.Pleasance == null ?
                                "(Unknown Mood)" :
                                $"(Pleasance: {teamObj.Pleasance:0.##} - Activation: {teamObj.Activation:0.##})";
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

        private string _moodString;
        public string MoodString
        {
            get => _moodString;
            set => SetProperty(ref _moodString, value);
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

        private string _moodIconImagePath;
        public string MoodIconImagePath
        {
            get => _moodIconImagePath;
            set => SetProperty(ref _moodIconImagePath, value);
        }
    }
}
