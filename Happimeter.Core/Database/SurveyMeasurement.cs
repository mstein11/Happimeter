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
        public int UserId { get; set; }

        public DateTime Timestamp { get; set; }
        public int IdFromWatch { get; set; }
        public DateTime? TimestampArrivedOnPhone { get; set; }

        public bool IsUploadedToServer { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<SurveyItemMeasurement> SurveyItemMeasurement { get; set; }

        public string WatchAppVersion { get; set; }
        public string PhoneAppVersion { get; set; }
        public int WatchBatteryPercentage { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Altitude { get; set; }
    }
}
