using System;
using System.Threading.Tasks;
using Happimeter.Events;
using Happimeter.Models.ServiceModels;

namespace Happimeter.Interfaces
{
    public interface IHappimeterApiService
    {
        Task<AuthResultModel> Auth(string email, string password);
        Task<RegisterUserResultModel> CreateAccount(string email, string password);
        Task<GetMeResultModel> GetMe();
        Task<HappimeterApiResultInformation> UploadMood();
        event EventHandler<SynchronizeDataEventArgs> UploadMoodStatusUpdate;
        Task<HappimeterApiResultInformation> UploadSensor();
        event EventHandler<SynchronizeDataEventArgs> UploadSensorStatusUpdate;
        Task<GetGenericQuestionApiResult> GetGenericQuestions();
        Task<GetPredictionsResultModel> GetPredictions();
        Task<GetProximityResultModel> GetProximityData(DateTime since);
        Task<GetTeamsResultModel> GetTeams();
        Task<GetSignalsModel> GetSignals(DateTime forDay);
        Task<bool> UploadDatabaseForDebug();
        Task<GetTeamsByNameResultModel> GetTeamsByName(string name);
        Task<JoinTeamResultModel> JoinTeam(int teamId, string password);
    }
}