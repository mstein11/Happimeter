using System.Collections.Generic;
using Happimeter.Core.Database;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Interfaces;

namespace Happimeter.Services
{
    public class MeasurementService : IMeasurementService
    {
        public MeasurementService()
        {
        }

        public void AddMeasurements(DataExchangeMessage message) {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();

            foreach (var measurement in message.SurveyMeasurements)
            {
                measurement.Id = 0;
                context.AddGraph(measurement);
            }
        }

        public List<SurveyMeasurement> GetSurveyMeasurements() {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            return context.GetAllWithChildren<SurveyMeasurement>();
        }
    }
}
