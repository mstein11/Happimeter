using System;
using System.Collections.Generic;
using Happimeter.Core.Helper;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Diagnostics;
using Happimeter.Core.Database;
using System.Linq;

namespace Happimeter.Core.Services
{
    public class LoggingService : ILoggingService
    {
        public const string LoginEvent = "Login";
        public const string LogoutEvent = "Logout";
        public const string PairEvent = "Pair";
        public const string PairFailureEvent = "PairFailure";
        public const string DataExchangeReceivedNotification = "DataExchangeReceivedNotification";
        public const string DataExchangeStart = "DataExchangeStart";
        public const string DataExchangeEnd = "DataExchangeEnd";
        public const string DataExchangeFailure = "DataExchangeFailure";
        public const string BeaconRegionEnteredEvent = "BeaconRegionEnteredEvent";
        public const string BackgroundTaskExpired = "BackgroundTaskExpired";
        public const string BeaconRegionLeftEvent = "BeaconRegionLeftEvent";
        public const string CouldNotUploadSensorNewFormat = "CouldNotUploadSensorNewFormat";
        public const string CouldNotUploadSensorOldFormat = "CouldNotUploadSensorOldFormat";
        public const string CouldNotFindCharacteristicsOnConnectedDevice = "CouldNotFindCharacteristicsOnConnectedDevice";

        public const string DebugSnapshot = "DebugSnapshot";


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
            var st = new StackTrace();
            var sf = st.GetFrame(1);
            var currentMethodName = sf.GetMethod();
            if (!data.ContainsKey("methodName") && currentMethodName != null)
            {
                data.Add("methodName", currentMethodName.DeclaringType.FullName + "." + currentMethodName.Name);
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
            var st = new StackTrace();
            var sf = st.GetFrame(1);
            var currentMethodName = sf.GetMethod();
            if (!data.ContainsKey("methodName") && currentMethodName != null)
            {
                data.Add("methodName", currentMethodName.DeclaringType.FullName + "." + currentMethodName.Name);
            }
            Crashes.TrackError(exception, data);
        }

        public void CreateDebugSnapshot()
        {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var allSurveys = context.GetAll<SurveyMeasurement>();
            var lastDataExchange = context.Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive)?.LastDataSync;
            var lastSensorDataEntry = context.GetSensorMeasurements(0, 1, true).FirstOrDefault()?.Timestamp;
            var lastMoodDataEntry = allSurveys.OrderBy(x => x.Timestamp).LastOrDefault()?.Timestamp;
            var totalSurveys = allSurveys.Count();
            var surveysNotUploaded = allSurveys.Where(x => !x.IsUploadedToServer).Count();
            var totalSensors = context.CountSensorMeasurements();
            var sensorsNotUploaded = context.CountSensorMeasurementsNotUploaded();

            var data = new Dictionary<string, string>();
            data.Add("lastDataExchange", lastDataExchange.ToString());
            data.Add("lastSensorDataEntry", lastSensorDataEntry.ToString());
            data.Add("lastMoodDataEntry", lastMoodDataEntry.ToString());
            data.Add("totalSurveys", totalSurveys.ToString());
            data.Add("surveysNotUploaded", surveysNotUploaded.ToString());
            data.Add("totalSensors", totalSensors.ToString());
            data.Add("sensorsNotUploaded", sensorsNotUploaded.ToString());

            LogEvent(DebugSnapshot, data);
        }
    }
}