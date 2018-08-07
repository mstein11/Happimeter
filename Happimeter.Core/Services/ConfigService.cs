using System;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Core.Models;
using System.Linq;

namespace Happimeter.Core.Services
{
    public class ConfigService : IConfigService
    {
        public const string GenericQuestionGroupIdKey = "GENERIC_QUESTION_GROUP";
        public const string WatchNameKey = "WATCH_NAME_KEY";
        public const string BatterySaferMeasurementInterval = "BATTERY_SAFER_MEASUREMENT_INTERVAL";
        public const string BatterySaferMeasurementIntervalId = "BATTERY_SAFER_MEASUREMENT_INTERVAL_ID";
        public const string DeactivateAppStartsOnBoot = "DEACTIVATE_APP_STARTS_ON_BOOT";

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


        /// <summary>
        ///     Enables the Continous mode. In continous mode, the watch constantly collects sensor data. 
        ///     Every minute, the watch calculated average, etc of the collected metrics and safes it to the db.
        /// </summary>
        /*public void SetContinousMeasurementMode()
        {
            var key = ConfigService.BatterySaferMeasurementInterval;
            RemoveConfigEntry(key);
        }*

        /// <summary>
        ///     Toggles the measurement mode of the watch.
        ///     Batterysafer mode is characterized by first gathering the sensor data for half of the given interval and then doing nothing the other half of the interval.
        ///     During the time where there is done nothing, the watch can go into hibernate mode to save battery.
        /// </summary>
        /// <param name="measurementInterval">Measurement interval.</param>
        /*public void SetBatterySaferMeasurementMode(int measurementInterval = 900)
        {
            var key = ConfigService.BatterySaferMeasurementInterval;
            AddOrUpdateConfigEntry(key, measurementInterval.ToString());
        }*/

        /// OLD
        /// <summary>
        ///     If this method retuns null, we are in Continous Mode.
        ///     If this method returns a int value. We are in BatterySaferMode and the returned values indicates the duration of one interval.
        /// </summary>
        /// <returns>The measurement mode.</returns>
        /*public int? GetMeasurementMode()
        {
            var key = ConfigService.BatterySaferMeasurementInterval;
            var value = GetConfigValueByKey(key);
            int outputValue;
            if (int.TryParse(value, out outputValue))
            {
                return outputValue;
            }
            return null;
        }*/

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
