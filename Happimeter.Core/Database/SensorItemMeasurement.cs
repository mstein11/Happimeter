using System;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Happimeter.Core.Database
{
    public class SensorItemMeasurement
    {
        public SensorItemMeasurement()
        {
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Type { get; set; }

        [ForeignKey(typeof(SensorMeasurement))]
        public int SensorMeasurementId { get; set; }
        [ManyToOne]
        public SensorMeasurement SensorMeasurement { get; set; }

        public int NumberOfMeasures { get; set; }
        public double Average { get; set; }
        public double StdDev { get; set; }
        public double Magnitude { get; set; }
        public double Quantile1 { get; set; }
        public double Quantile2 { get; set; }
        public double Quantile3 { get; set; }

        public double Quantile1Amount { get; set; }
        public double Quantile2Amount { get; set; }
        public double Quantile3Amount { get; set; }
        public double HighestValuesAmount { get; set; }
    }
}
