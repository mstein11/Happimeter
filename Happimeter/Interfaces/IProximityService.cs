using System.Collections.Generic;
using System.Threading.Tasks;
using Happimeter.Core.Database;
using System;

namespace Happimeter.Interfaces
{
    public interface IProximityService
    {
        Task DownloadAndSaveProximity();
        List<ProximityEntry> GetProximityEntries(DateTime? forday = null);
    }
}
