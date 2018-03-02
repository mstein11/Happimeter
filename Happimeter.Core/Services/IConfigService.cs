namespace Happimeter.Core.Services
{
    public interface IConfigService
    {
        void AddOrUpdateConfigEntry(string key, string value);
        string GetConfigValueByKey(string key);
        void RemoveConfigEntry(string key);
    }
}