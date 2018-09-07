using System;
using Happimeter.Core.Database;
using Happimeter.Interfaces;
using Happimeter.Core.Helper;
using System.Reactive.Subjects;
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
            LeaveTeamCommand = new Command(() =>
            {
                _teamService.LeaveTeam(Id);
                //todo: check if successfull
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

        public Command LeaveTeamCommand { get; set; }
        private readonly Subject<int> TeamLeftSubject = new Subject<int>();
        public IObservable<int> WhenTeamLeft()
        {
            return TeamLeftSubject;
        }
    }
}
