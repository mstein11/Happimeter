using System;
using System.Threading.Tasks;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using System.Linq;
using Happimeter.Core.Database;
using System.Collections.Generic;

namespace Happimeter.Services
{
    public class ProximityService : IProximityService
    {
        public ProximityService()
        {
        }

        public async Task DownloadAndSaveProximity()
        {
            var lastEntry = ServiceLocator.Instance.Get<ISharedDatabaseContext>().GetAll<ProximityEntry>().OrderBy(x => x.Timestamp).LastOrDefault();
            var lastEntryDate = lastEntry?.Timestamp ?? default(DateTime);
            var proximity = await ServiceLocator.Instance.Get<IHappimeterApiService>().GetProximityData(lastEntryDate);
            if (!proximity.IsSuccess)
            {
                return;
            }

            var dbObjs = proximity.Data
                                 .Select(x =>
                                         new ProximityEntry
                                         {
                                             CloseToUserId = x.CloseToUserId,
                                             Average = x.Average,
                                             Timestamp = x.Timestamp,
                                             CloseToUserIdentifier = string.IsNullOrEmpty(x.Name) ? x.Mail : x.Name
                                         });
            foreach (var entry in dbObjs)
            {
                ServiceLocator.Instance.Get<ISharedDatabaseContext>().Add(entry);
            }
        }

        public List<ProximityEntry> GetProximityEntries(DateTime? forDay = null)
        {

            if (forDay == null)
            {
                return ServiceLocator.Instance.Get<ISharedDatabaseContext>().GetAll<ProximityEntry>();
            }

            var forDayLocalTime = new DateTime(forDay.Value.Year, forDay.Value.Month, forDay.Value.Day);
            var from = forDayLocalTime.Date.ToUniversalTime();
            var to = forDayLocalTime.Date.AddHours(24).ToUniversalTime();
            return ServiceLocator.Instance.Get<ISharedDatabaseContext>().GetAll<ProximityEntry>(x => x.Timestamp >= from && x.Timestamp <= to);
        }
    }
}
