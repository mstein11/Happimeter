using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Happimeter.Core.Database;
using Happimeter.Services;

namespace Happimeter.Interfaces
{
    public interface ITeamService
    {
        Task DownloadAndSave();
        IList<TeamEntry> GetTeams();
        TeamEntry GetTeam(int id);
        Task<(JoinTeamResult, int?)> JoinTeam(string name, string password);
        Task<bool> LeaveTeam(int id);
        IObservable<IList<TeamEntry>> WhenTeamAdded();
        IObservable<IList<TeamEntry>> WhenTeamChanged();
        IObservable<IList<TeamEntry>> WhenTeamDeleted();
    }
}