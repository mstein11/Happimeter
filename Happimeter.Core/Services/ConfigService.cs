using System;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;

namespace Happimeter.Core.Services
{
    public class ConfigService : IConfigService
    {
        public const string GenericQuestionGroupIdKey = "GENERIC_QUESTION_GROUP";
        public const string WatchNameKey = "WATCH_NAME_KEY";

        public void AddOrUpdateConfigEntry(string key, string value) {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();

            var entry = context.Get<ConfigEntry>(x => x.Key == key);
            if (entry != null) 
            {
                entry.Value = value;
                context.Update(entry);    
            } else {
                var newEntry = new ConfigEntry
                {
                    Key = key,
                    Value = value
                };
                context.Add(newEntry);
            }
        }

        public void RemoveConfigEntry(string key) {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var entry = context.Get<ConfigEntry>(x => x.Key == key);
            if (entry != null)
            {
                context.Delete(entry);
            }
        }

        public string GetConfigValueByKey(string key) {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var entry = context.Get<ConfigEntry>(x => x.Key == key);
            return entry?.Value ?? null;
        }
    }
}
