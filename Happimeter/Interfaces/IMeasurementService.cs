using System.Collections.Generic;
using System.Threading.Tasks;
using Happimeter.Core.Database;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Models.ServiceModels;
using Happimeter.ViewModels.Forms;

namespace Happimeter.Interfaces
{
    public interface IMeasurementService
    {
        SurveyViewModel GetSurveyQuestions();
        void AddMeasurements(DataExchangeMessage message);
        void AddSurveyData(SurveyViewModel model);
        List<SurveyMeasurement> GetSurveyData();
        List<SurveyMeasurement> GetSurveyMeasurements();
        List<PostMoodServiceModel> GetSurveyModelForServer();
        void SetIsUploadedToServerForSurveys(PostMoodServiceModel survey);
        (List<PostSensorDataServiceModel>, List<SensorMeasurement>) GetSensorDataForServer();
        bool HasUnsynchronizedSensorData();
        int CountUnsynchronizedSensorData();
        bool HasUnsynchronizedSurveyData();
        int CountUnsynchronizedSurveyData();
        void SetIsUploadedToServerForSensorData(PostSensorDataServiceModel sensor);
        bool HasUnsynchronizedChanges();
        Task<List<GenericQuestion>> DownloadAndSaveGenericQuestions();
        MyTabMenuViewModel GetQuestionsToDisplayInTabMenu();
    }
}