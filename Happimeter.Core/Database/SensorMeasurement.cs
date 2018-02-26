using System;
using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Happimeter.Core.Database
{
    public class SensorMeasurement
    {
        public SensorMeasurement()
        {
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsUploadedToServer { get; set; }

        public int IdFromWatch { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<SensorItemMeasurement> SensorItemMeasures { get; set; }
    }
}
