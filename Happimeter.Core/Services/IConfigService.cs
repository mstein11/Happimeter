using Happimeter.Core.Models;

namespace Happimeter.Core.Services
{
    public interface IConfigService
    {
        void AddOrUpdateConfigEntry(string key, string value);
        string GetConfigValueByKey(string key);
        void RemoveConfigEntry(string key);
        MeasurementModeModel GetMeasurementMode();
        void SetMeasurementMode(int id);
        bool IsContinousMeasurementMode();
        void SetDeactivateAppStartsOnBoot(bool starts);
        bool GetDeactivateAppStartsOnBoot();
        bool GetSaveMeasurementIfNoHeartrate();
        void SetSaveMeasurementIfNoHeartrate(bool saveMeasurementIfNoHeartrate);
    }
}