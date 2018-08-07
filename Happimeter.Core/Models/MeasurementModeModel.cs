using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Happimeter.Core.Models
{
    public class MeasurementModeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? IntervalSeconds { get; set; }
        public int? FactorMeasurementOfInterval { get; set; }
        public bool IsSelected { get; set; }

        public MeasurementModeModel GetModeById(int id)
        {
            return GetModes().FirstOrDefault(x => x.Id == id);
        }

        public static MeasurementModeModel GetDefault() => GetModes().FirstOrDefault();

        public static IList<MeasurementModeModel> GetModes()
        {
            return new List<MeasurementModeModel> {
                new MeasurementModeModel {
                    Id = 1,
                    Name = "Super Battery Safer",
                    IntervalSeconds = 1800, //30 minutes
                    FactorMeasurementOfInterval = 60,
                    Description = "Every 30 Minutes the sensors run for 30 seconds."
                },
                new MeasurementModeModel {
                    Id = 2,
                    Name = "Battery Safer",
                    IntervalSeconds = 900, //15 minutes
                    FactorMeasurementOfInterval = 30,
                    Description = "Every 15 Minutes the sensors run for 30 seconds."
                },
                new MeasurementModeModel {
                    Id = 3,
                    Name = "Normal Mode",
                    IntervalSeconds = 300, // 5 minutes
                    FactorMeasurementOfInterval = 10,
                    Description = "Every 5 Minutes the sensors run for 30 seconds."
                },
                new MeasurementModeModel {
                    Id = 4,
                    Name = "Continuous mode",
                    IntervalSeconds = null, // every minute with continous measurements
                    FactorMeasurementOfInterval = null,
                    Description = "The sensors run continuously and a aggregated measurement is saved every minute"
                }
            };
        }
    }
}
