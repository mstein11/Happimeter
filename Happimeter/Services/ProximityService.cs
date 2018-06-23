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
		private bool IsDownloadingAndSaving = false;
		public ProximityService()
		{
		}


		public async Task DownloadAndSaveProximity()
		{
			//this operation might take a few seconds, we lock this method so that the user does not download the stuff multiple times
			if (IsDownloadingAndSaving)
			{
				return;
			}
			IsDownloadingAndSaving = true;
			try
			{
				var lastEntry = ServiceLocator.Instance.Get<ISharedDatabaseContext>().GetAll<ProximityEntry>().OrderBy(x => x.Timestamp).LastOrDefault();
				var lastEntryDate = lastEntry?.Timestamp ?? default(DateTime);
				if (DateTime.UtcNow - lastEntryDate > TimeSpan.FromDays(20))
				{
					lastEntryDate = DateTime.UtcNow - TimeSpan.FromDays(20);
				}
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
												 ProximityType = x.Type.Split('_')[0],
												 Timestamp = x.Timestamp,
												 CloseToUserIdentifier = string.IsNullOrEmpty(x.Name) ? x.Mail : x.Name

											 });
				await Task.Factory.StartNew(() =>
				{
					foreach (var entry in dbObjs)
					{
						ServiceLocator.Instance.Get<ISharedDatabaseContext>().Add(entry);
					}
				});
			}
			finally
			{
				IsDownloadingAndSaving = false;
			}
		}

		public List<ProximityEntry> GetProximityEntries(DateTime? forDay = null)
		{
			if (forDay != null)
			{
				var forDayLocalTime = new DateTime(forDay.Value.Year, forDay.Value.Month, forDay.Value.Day);
				var from = forDayLocalTime.Date.ToUniversalTime();
				var to = forDayLocalTime.Date.AddHours(24).ToUniversalTime();
				return ServiceLocator.Instance.Get<ISharedDatabaseContext>().GetAll<ProximityEntry>(x => x.Timestamp >= from && x.Timestamp <= to);
			}
			else
			{
				return ServiceLocator.Instance.Get<ISharedDatabaseContext>().GetAll<ProximityEntry>();
			}
		}

		public IList<SensorMeasurement> GetProximityCm(DateTime? forDay = null)
		{
			return ServiceLocator.Instance.Get<ISharedDatabaseContext>().GetProximity(forDay);
		}
	}
}
