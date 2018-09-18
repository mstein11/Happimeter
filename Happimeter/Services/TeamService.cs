using Happimeter.Core.Helper;
using Happimeter.Core.Database;
using System.Threading.Tasks;
using Happimeter.Interfaces;
using System.Linq;
using System.Collections.Generic;
using System;
using Happimeter.Core.Services;
using System.Reactive.Linq;

namespace Happimeter.Services
{
    public class TeamService : ITeamService
    {
        private readonly ISharedDatabaseContext _sharedDatabaseContext;
        private readonly IHappimeterApiService _apiService;
        private readonly ILoggingService _loggingService;
        private readonly IGenericQuestionService _genericQuestionService;
        public TeamService()
        {
            _sharedDatabaseContext = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            _apiService = ServiceLocator.Instance.Get<IHappimeterApiService>();
            _loggingService = ServiceLocator.Instance.Get<ILoggingService>();
            _genericQuestionService = ServiceLocator.Instance.Get<IGenericQuestionService>();

        }

        public async Task DownloadAndSave()
        {
            try
            {
                var apiResponse = await _apiService.GetTeams();
                if (apiResponse == null || apiResponse.Status != 200)
                {
                    return;
                }
                var oldTeams = _sharedDatabaseContext.GetAll<TeamEntry>();

                var teams = apiResponse.Teams.Select(x => new TeamEntry
                {
                    Id = x.Id,
                    IsAdmin = x.IsAdmin,
                    Name = x.Name,
                    Activation = x.Mood.Activation,
                    Pleasance = x.Mood.Pleasance
                }).ToList();

                foreach (var team in teams)
                {
                    var oldCurrentTeam = oldTeams.FirstOrDefault(x => x.Id == team.Id);
                    if (oldCurrentTeam != null)
                    {
                        oldTeams.Remove(oldCurrentTeam);
                        oldCurrentTeam.IsAdmin = team.IsAdmin;
                        oldCurrentTeam.Name = team.Name;
                        oldCurrentTeam.Activation = team.Activation;
                        oldCurrentTeam.Pleasance = team.Pleasance;
                        _sharedDatabaseContext.Update(oldCurrentTeam);
                    }
                    else
                    {
                        _sharedDatabaseContext.Add(team);
                    }
                }
                foreach (var oldTeamToDelete in oldTeams)
                {
                    _sharedDatabaseContext.Delete(oldTeamToDelete);
                }
            }
            catch (Exception e)
            {
                _loggingService.LogException(e);
                throw e;
            }
        }

        public IList<TeamEntry> GetTeams()
        {
            return _sharedDatabaseContext.GetAll<TeamEntry>();
        }

        public TeamEntry GetTeam(int id)
        {
            return _sharedDatabaseContext.Get<TeamEntry>(x => x.Id == id);
        }

        public async Task<bool> LeaveTeam(int id)
        {
            var res = await _apiService.LeaveTeam(id);
            if (!res.IsSuccess)
            {
                return false;
            }
            _sharedDatabaseContext.Delete(GetTeam(id));
            return true;
        }

        public async Task<(JoinTeamResult, int?)> JoinTeam(string name, string password)
        {
            var apiResponse = await _apiService.GetTeamsByName(name);
            if (!apiResponse.IsSuccess || apiResponse.Status != 200)
            {
                //todo:
                return (JoinTeamResult.InternetError, null);
            }
            else if (!apiResponse.Teams.Any())
            {
                //todo:
                return (JoinTeamResult.WrongTeam, null);
            }

            //api uses a LIKE to get the teams but it does not order the results by likelyness, the result seem to be ordered randomly
            //to ensure, that we use a perfect match is available, we need the below code
            var bestMatch = apiResponse.Teams.FirstOrDefault(x => x.Name.ToUpper() == name.ToUpper());
            if (bestMatch == null)
            {
                bestMatch = apiResponse.Teams.FirstOrDefault();
            }
            if (GetTeams().Any(x => x.Id == bestMatch.Id))
            {
                return (JoinTeamResult.AlreadyMember, bestMatch.Id);
            }
            var result = await _apiService.JoinTeam(bestMatch.Id, password);
            if (result.ResultType != Models.ServiceModels.HappimeterApiResultInformation.Success)
            {
                return (JoinTeamResult.InternetError, null);
            }
            else if (!result.IsSuccess)
            {
                return (JoinTeamResult.WrongPassword, null);
            }

            await DownloadAndSave();
            await _genericQuestionService.DownloadAndSaveGenericQuestions();
            return (JoinTeamResult.Success, bestMatch.Id);
        }

        public IObservable<IList<TeamEntry>> WhenTeamAdded()
        {
            return _sharedDatabaseContext
                .WhenEntryAdded<TeamEntry>()
                .Select(x => x.Entites.Cast<TeamEntry>().ToList());
        }

        public IObservable<IList<TeamEntry>> WhenTeamChanged()
        {
            return _sharedDatabaseContext
                .WhenEntryChanged<TeamEntry>()
                .Select(x => x.Entites.Cast<TeamEntry>().ToList());
        }

        public IObservable<IList<TeamEntry>> WhenTeamDeleted()
        {
            return _sharedDatabaseContext
                .WhenEntryDeleted<TeamEntry>()
                .Select(x => x.Entites.Cast<TeamEntry>().ToList());
        }
    }

    public enum JoinTeamResult
    {
        Success,
        InternetError,
        UnknownError,
        WrongTeam,
        WrongPassword,
        AlreadyMember
    }
}
