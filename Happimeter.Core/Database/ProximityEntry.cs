using System;
using SQLite;

namespace Happimeter.Core.Database
{
    public class ProximityEntry
    {
        public ProximityEntry()
        {
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double Average { get; set; }
        [Indexed]
        public DateTime Timestamp { get; set; }
        [Indexed]
        public int CloseToUserId { get; set; }
        public string CloseToUserIdentifier { get; set; }
    }
}
