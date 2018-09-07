using Happimeter.Core.Helper;
using Happimeter.Core.Database;
using System.Threading.Tasks;
using Happimeter.Interfaces;
using System.Linq;
using System.Collections.Generic;
using System;
using Happimeter.Core.Services;
using System.Security.Policy;

namespace Happimeter.Services
{
    public class TeamService : ITeamService
    {
        private readonly ISharedDatabaseContext _sharedDatabaseContext;
        private readonly IHappimeterApiService _apiService;
        private readonly ILoggingService _loggingService;
        public TeamService()
        {
            _sharedDatabaseContext = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            _apiService = ServiceLocator.Instance.Get<IHappimeterApiService>();
            _loggingService = ServiceLocator.Instance.Get<ILoggingService>();
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
                    Name = x.Name
                }).ToList();

                foreach (var team in teams)
                {
                    var oldCurrentTeam = oldTeams.FirstOrDefault(x => x.Id == team.Id);
                    if (oldCurrentTeam != null)
                    {
                        oldCurrentTeam.IsAdmin = team.IsAdmin;
                        oldCurrentTeam.Name = team.Name;
                        _sharedDatabaseContext.Update(oldCurrentTeam);
                    }
                    else
                    {
                        _sharedDatabaseContext.Add(team);
                    }
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

        public void LeaveTeam()
        {
            //todo: 
        }

        public async Task<JoinTeamResult> JoinTeam(string name, string password)
        {
            var apiResponse = await _apiService.GetTeamsByName(name);
            if (!apiResponse.IsSuccess || apiResponse.Status != 200)
            {
                //todo:
                return JoinTeamResult.InternetError;
            }
            else if (!apiResponse.Teams.Any())
            {
                //todo:
                return JoinTeamResult.WrongTeam;
            }

            //api uses a LIKE to get the teams but it does not order the results by likelyness, the result seem to be ordered randomly
            //to ensure, that we use a perfect match is available, we need the below code
            var bestMatch = apiResponse.Teams.FirstOrDefault(x => x.Name == name);
            if (bestMatch == null)
            {
                bestMatch = apiResponse.Teams.FirstOrDefault();
            }
            var result = await _apiService.JoinTeam(bestMatch.Id, password);
            if (result.ResultType != Models.ServiceModels.HappimeterApiResultInformation.Success)
            {
                return JoinTeamResult.InternetError;
            }
            else if (!result.IsSuccess)
            {
                return JoinTeamResult.WrongPassword;
            }

            await DownloadAndSave();
            return JoinTeamResult.Success;
        }
    }

    public enum JoinTeamResult
    {
        Success,
        InternetError,
        UnknownError,
        WrongTeam,
        WrongPassword
    }
}
