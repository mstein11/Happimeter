using System.Collections.Generic;
using Happimeter.Core.Database;
using Happimeter.Core.Models.Bluetooth;

namespace Happimeter.Interfaces
{
    public interface IMeasurementService
    {
        void AddMeasurements(DataExchangeMessage message);
        List<SurveyMeasurement> GetSurveyMeasurements();
    }
}