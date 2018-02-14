using Happimeter.Core.Database;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.ViewModels;

namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
    public interface IMeasurementService
    {
        void AddSurveyMeasurement(SurveyMeasurement measurement);
        SurveyViewModel GetSurveyQuestions();
        DataExchangeMessage GetMeasurementsForDataTransfer();
        void DeleteSurveyMeasurement(DataExchangeMessage message);
    }
}