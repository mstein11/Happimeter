using System;
using System.Collections.ObjectModel;
using Happimeter.Interfaces;
using Happimeter.Core.Helper;
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
                Teams.Add(new TeamListItemViewModel(team));
            }
        }

        private ObservableCollection<TeamListItemViewModel> _teams = new ObservableCollection<TeamListItemViewModel>();
        public ObservableCollection<TeamListItemViewModel> Teams
        {
            get => _teams;
            set => SetProperty(ref _teams, value);
        }

        private JoinTeamViewModel _joinTeamViewModel = new JoinTeamViewModel();
        public JoinTeamViewModel JoinTeamViewModel
        {
            get => _joinTeamViewModel;
            set => SetProperty(ref _joinTeamViewModel, value);
        }
    }
}
