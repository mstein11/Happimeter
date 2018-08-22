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
        IList<GenericQuestion> GetActiveGenericQuestions();
        Task AddMeasurements(DataExchangeMessage message);
        Task AddSurveyData(SurveyViewModel model);
        List<SurveyMeasurement> GetSurveyData(System.DateTime? from = null, System.DateTime? to = null);
        List<SurveyMeasurement> GetSurveyMeasurements();
        List<PostMoodServiceModel> GetSurveyModelForServer();
        void SetIsUploadedToServerForSurveys(PostMoodServiceModel survey);
        (List<PostSensorDataServiceModel>, List<SensorMeasurement>) GetSensorDataForServer();
        List<SensorMeasurement> GetSensorDataForListView(int skip = 0, int take = 100);
        bool HasUnsynchronizedSensorData();
        int CountUnsynchronizedSensorData();
        bool HasUnsynchronizedSurveyData();
        int CountUnsynchronizedSurveyData();
        void SetIsUploadedToServerForSensorData(PostSensorDataServiceModel sensor);
        bool HasUnsynchronizedChanges();
        Task<List<GenericQuestion>> DownloadAndSaveGenericQuestions();
        MyTabMenuViewModel GetQuestionsToDisplayInTabMenu();
        IList<GenericQuestion> GetGenericQuestions();
        void ToggleGenericQuestionActivation(int questionId, bool isActivated);
    }
}