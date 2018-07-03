using System;
using System.Collections.Generic;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace Happimeter.Services
{
	public class LoggingService : ILoggingService
	{
		public const string LoginEvent = "Login";
		public const string PairEvent = "Pair";
		public const string PairFailureEvent = "PairFailure";
		public const string DataExchangeStart = "DataExchangeStart";
		public const string DataExchangeEnd = "DataExchangeEnd";
		public const string DataExchangeFailure = "DataExchangeFailure";
		public const string BeaconRegionEnteredEvent = "BeaconRegionEnteredEvent";
		public const string BackgroundTaskExpired = "BackgroundTaskExpired";
		public const string BeaconRegionLeftEvent = "BeaconRegionLeftEvent";
		public const string CouldNotUploadSensorNewFormat = "CouldNotUploadSensorNewFormat";
		public const string CouldNotUploadSensorOldFormat = "CouldNotUploadSensorOldFormat";


		public void LogEvent(string name, Dictionary<string, string> data = null)
		{
			if (data == null)
			{
				data = new Dictionary<string, string>();
			}
			var accountStore = ServiceLocator.Instance.Get<IAccountStoreService>();
			var username = accountStore.GetAccount()?.Username ?? "NO-USERNAME";
			if (!data.ContainsKey("user"))
			{
				data.Add("user", username);
			}
			Analytics.TrackEvent(name, data);
		}

		public void LogException(Exception exception, Dictionary<string, string> data = null)
		{
			if (data == null)
			{
				data = new Dictionary<string, string>();
			}
			var accountStore = ServiceLocator.Instance.Get<IAccountStoreService>();
			var username = accountStore.GetAccount()?.Username ?? "NO-USERNAME";
			if (!data.ContainsKey("user"))
			{
				data.Add("user", username);
			}
			Crashes.TrackError(exception, data);
		}
	}
}