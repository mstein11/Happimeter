namespace Happimeter.Core.Services
{
    public interface IConfigService
    {
        void AddOrUpdateConfigEntry(string key, string value);
        string GetConfigValueByKey(string key);
        void RemoveConfigEntry(string key);
        void SetContinousMeasurementMode();
        void SetBatterySaferMeasurementMode(int measurementInterval = 600);
        int? GetMeasurementMode();
        bool IsContinousMeasurementMode();
    }
}