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
        Task<JoinTeamResult> JoinTeam(string name, string password);
        void LeaveTeam();
    }
}