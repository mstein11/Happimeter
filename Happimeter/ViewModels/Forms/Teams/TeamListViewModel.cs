using System;
using System.Collections.ObjectModel;
using Happimeter.Interfaces;
using Happimeter.Core.Helper;
using System.Linq;
using System.Reactive.Subjects;

namespace Happimeter.ViewModels.Forms.Teams
{
    public class TeamListViewModel : BaseViewModel
    {
        private readonly ITeamService _teamService;
        public TeamListViewModel()
        {
            _teamService = ServiceLocator.Instance.Get<ITeamService>();

            var dbTeams = _teamService.GetTeams();

            foreach (var team in dbTeams)
            {
                var itemViewModel = new TeamListItemViewModel(team);
                Teams.Add(itemViewModel);
                IDisposable subsciption;
                subsciption = itemViewModel.WhenTeamLeft().Subscribe(x =>
                {
                    var teamToRemove = Teams.FirstOrDefault(t => t.Id == x);
                    Teams.Remove(teamToRemove);
                });
            }
            JoinTeamViewModel.WhenTeamSuccessfullyJoined().Subscribe(teamId =>
            {
                var team = _teamService.GetTeam(teamId);
                Teams.Add(new TeamListItemViewModel(team));
            });
        }

        private ObservableCollection<TeamListItemViewModel> _teams = new ObservableCollection<TeamListItemViewModel>();
        public ObservableCollection<TeamListItemViewModel> Teams
        {
            get => _teams;
            set => SetProperty(ref _teams, value);
        }

        private readonly Subject<int> TeamLeftSubject = new Subject<int>();
        public IObservable<int> WhenTeamLeft()
        {
            return TeamLeftSubject;
        }

        private JoinTeamViewModel _joinTeamViewModel = new JoinTeamViewModel();
        public JoinTeamViewModel JoinTeamViewModel
        {
            get => _joinTeamViewModel;
            set => SetProperty(ref _joinTeamViewModel, value);
        }
    }
}
