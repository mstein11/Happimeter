﻿using System;
namespace Happimeter.Core.Database.ModelsNotMappedToDb
{
    public class SensorMeasurementsAndItems
    {
        public SensorMeasurementsAndItems()
        {
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsUploadedToServer { get; set; }

        public string WatchAppVersion { get; set; }
        public string PhoneAppVersion { get; set; }
        public int WatchBatteryPercentage { get; set; }

        public int IdFromWatch { get; set; }

        public string Type { get; set; }

        public int SensorMeasurementId { get; set; }

        public int NumberOfMeasures { get; set; }
        public double Average { get; set; }
        public double StdDev { get; set; }
        public double Magnitude { get; set; }
        public double Quantile1 { get; set; }
        public double Quantile2 { get; set; }
        public double Quantile3 { get; set; }

        public double Min { get; set; }
        public double Max { get; set; }


    }
}
