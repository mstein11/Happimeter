namespace Happimeter.Core.Services
{
    public interface IConfigService
    {
        void AddOrUpdateConfigEntry(string key, string value);
        string GetConfigValueByKey(string key);
        void RemoveConfigEntry(string key);
        void SetContinousMeasurementMode();
        void SetBatterySaferMeasurementMode(int measurementInterval = 300);
        int? GetMeasurementMode();
        bool IsContinousMeasurementMode();
        void SetDeactivateAppStartsOnBoot(bool starts);
        bool GetDeactivateAppStartsOnBoot();
    }
}