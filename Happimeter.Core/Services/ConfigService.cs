using System;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Core.Models;
using System.Linq;

namespace Happimeter.Core.Services
{
    public class ConfigService : IConfigService
    {
        public const string WatchNameKey = "WATCH_NAME_KEY";
        public const string BatterySaferMeasurementIntervalId = "BATTERY_SAFER_MEASUREMENT_INTERVAL_ID";
        public const string DeactivateAppStartsOnBoot = "DEACTIVATE_APP_STARTS_ON_BOOT";
        public const string NotificationDeviceToken = "NOTIFICATION_DEVICE_TOKEN";

        public void AddOrUpdateConfigEntry(string key, string value)
        {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();

            var entry = context.Get<ConfigEntry>(x => x.Key == key);
            if (entry != null)
            {
                entry.Value = value;
                context.Update(entry);
            }
            else
            {
                var newEntry = new ConfigEntry
                {
                    Key = key,
                    Value = value
                };
                context.Add(newEntry);
            }
        }

        public void RemoveConfigEntry(string key)
        {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var entry = context.Get<ConfigEntry>(x => x.Key == key);
            if (entry != null)
            {
                context.Delete(entry);
            }
        }

        public string GetConfigValueByKey(string key)
        {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var entry = context.Get<ConfigEntry>(x => x.Key == key);
            return entry?.Value ?? null;
        }

        public MeasurementModeModel GetMeasurementMode()
        {
            var id = GetConfigValueByKey(BatterySaferMeasurementIntervalId);
            int idAsInt;
            if (id == null || !int.TryParse(id, out idAsInt))
            {
                return MeasurementModeModel.GetDefault();
            }
            var mode = MeasurementModeModel.GetModes().FirstOrDefault(x => x.Id == idAsInt);
            return mode ?? MeasurementModeModel.GetDefault();
        }

        public void SetMeasurementMode(int id)
        {
            AddOrUpdateConfigEntry(BatterySaferMeasurementIntervalId, id.ToString());
        }

        /// <summary>
        ///     Helper method to identify wheater watch runs in contonous or battery saver mode.
        /// </summary>
        /// <returns><c>true</c>, if continous measurement mode was ised, <c>false</c> if runnign in battery saver mode.</returns>
        public bool IsContinousMeasurementMode()
        {
            return GetMeasurementMode().IntervalSeconds == null;
        }

        public void SetDeactivateAppStartsOnBoot(bool starts)
        {
            AddOrUpdateConfigEntry(DeactivateAppStartsOnBoot, starts.ToString());
        }

        public bool GetDeactivateAppStartsOnBoot()
        {
            return GetConfigValueByKey(DeactivateAppStartsOnBoot) == true.ToString();
        }
    }
}
