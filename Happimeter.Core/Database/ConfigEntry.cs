using System;
using SQLite;

namespace Happimeter.Core.Database
{
    public class ConfigEntry
    {
        public ConfigEntry()
        {
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
