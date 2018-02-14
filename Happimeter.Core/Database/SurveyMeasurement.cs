using System;
using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Happimeter.Core.Database
{
    public class SurveyMeasurement
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<SurveyItemMeasurement> SurveyItemMeasurement { get; set; }
    }
}
