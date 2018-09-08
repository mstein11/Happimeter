using System;
using System.Collections.ObjectModel;
using Happimeter.Interfaces;
using Happimeter.Core.Helper;
using System.Linq;
using System.Reactive.Subjects;
using Happimeter.Core.Database;
using Xamarin.Forms;
using System.Runtime.Serialization.Formatters;

namespace Happimeter.ViewModels.Forms.Teams
{
    public class TeamListViewModel : BaseViewModel
    {
        private readonly ITeamService _teamService;
        private readonly ISharedDatabaseContext _databaseContext;
        public TeamListViewModel()
        {
            _teamService = ServiceLocator.Instance.Get<ITeamService>();

            var dbTeams = _teamService.GetTeams();
            foreach (var team in dbTeams)
            {
                var itemViewModel = InstantiateTeamListItemViewModel(team);
                Teams.Add(itemViewModel);
            }
            JoinTeamViewModel.WhenTeamSuccessfullyJoined().Subscribe(teamId =>
            {
                if (Teams.Any(x => x.Id == teamId))
                {
                    //team already joined through database event
                    return;
                }
                var team = _teamService.GetTeam(teamId);
                var teamVm = InstantiateTeamListItemViewModel(team);
                Teams.Add(teamVm);
                OnPropertyChanged(nameof(Teams));
            });

            RefreshListCommand = new Command(() =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    IsRefreshing = true;
                    await _teamService.DownloadAndSave();
                    IsRefreshing = false;
                });
            });
            SetupDatabaseChangeEvents();
        }

        private void SetupDatabaseChangeEvents()
        {
            _teamService.WhenTeamAdded().Subscribe(teams =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (var team in teams)
                    {
                        if (Teams.Any(x => x.Id == team.Id))
                        {
                            continue;
                            //team already added
                        }
                        Teams.Add(InstantiateTeamListItemViewModel(team));
                    }
                    OnPropertyChanged(nameof(Teams));
                });
            });
            _teamService.WhenTeamDeleted().Subscribe(teamsDeleted =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (var team in teamsDeleted)
                    {
                        var toRemove = Teams.FirstOrDefault(x => x.Id == team.Id);
                        if (toRemove != null)
                        {
                            Teams.Remove(toRemove);
                        }
                        OnPropertyChanged(nameof(Teams));
                    }
                });
            });
            _teamService.WhenTeamChanged().Subscribe(teamschanged =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (var team in teamschanged)
                    {
                        var toChange = Teams.FirstOrDefault(x => x.Id == team.Id);
                        if (toChange != null)
                        {
                            toChange.Update(team);
                        }
                    }
                });
            });
        }

        private TeamListItemViewModel InstantiateTeamListItemViewModel(TeamEntry team)
        {
            var teamVm = new TeamListItemViewModel(team);
            IDisposable subsciption = null;
            subsciption = teamVm.WhenTeamLeft().Subscribe(x =>
            {
                var teamToRemove = Teams.FirstOrDefault(t => t.Id == x);
                Teams.Remove(teamToRemove);
                OnPropertyChanged(nameof(Teams));
                subsciption.Dispose();
            });
            return teamVm;
        }

        public Command RefreshListCommand { get; set; }
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
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
