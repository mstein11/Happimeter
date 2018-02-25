using System;
using System.Collections.Generic;
using System.Linq;
using Happimeter.Core.Database;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Helpers;
using Happimeter.Interfaces;
using Happimeter.Models.ServiceModels;
using Happimeter.ViewModels.Forms;

namespace Happimeter.Services
{
    public class MeasurementService : IMeasurementService
    {
        public MeasurementService()
        {
        }

        public SurveyViewModel GetSurveyQuestions()
        {
            var questions = new SurveyViewModel();
            var question1 = new SurveyItemViewModel
            {
                Question = "How Pleasant do you feel?",
                HardcodedId = 1,
                Answer = .5
            };
            var question2 = new SurveyItemViewModel
            {
                Question = "How Active do you feel?",
                HardcodedId = 2,
                Answer = .5
            };
            var question3 = new SurveyItemViewModel
            {
                Question = "How Stressed do you feel?",
                Answer = .5
            };

            questions.SurveyItems.Add(question1);
            questions.SurveyItems.Add(question2);
            questions.SurveyItems.Add(question3);

            return questions;
        }

        public void AddMeasurements(DataExchangeMessage message) {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var apiService = ServiceLocator.Instance.Get<IHappimeterApiService>();

            foreach (var measurement in message.SurveyMeasurements)
            {
                measurement.IdFromWatch = measurement.Id;
                measurement.Id = 0;
                context.AddGraph(measurement);
            }

            if (message.SurveyMeasurements.Any()) {
                apiService.UploadMoad();
            }

            foreach (var measurement in message.SensorMeasurements)
            {
                measurement.IdFromWatch = measurement.Id;
                measurement.Id = 0;
                context.AddGraph(measurement);
            }
        }

        public void AddSurveyData(SurveyViewModel model) {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var surveyMeasurement = new SurveyMeasurement
            {
                IdFromWatch = -1,
                Timestamp = DateTime.UtcNow,
                SurveyItemMeasurement = new List<SurveyItemMeasurement>()
            };

            foreach (var item in model.SurveyItems) {
                surveyMeasurement.SurveyItemMeasurement.Add(new SurveyItemMeasurement
                {
                    Answer = (int)(item.Answer * 100),
                    HardcodedQuestionId = item.HardcodedId,
                    Question = item.Question,
                    AnswerDisplay = item.AnswerDisplay,
                });  
            }

            context.AddGraph(surveyMeasurement);
            var apiService = ServiceLocator.Instance.Get<IHappimeterApiService>();
            apiService.UploadMoad();
        }

        public List<SurveyMeasurement> GetSurveyData() {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            return context.GetAllWithChildren<SurveyMeasurement>().ToList();
        }

        public List<PostMoodServiceModel> GetSurveyModelForServer() {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var entries = context.GetAllWithChildren<SurveyMeasurement>(x => !x.IsUploadedToServer);

            var result = new List<PostMoodServiceModel>();

            foreach (var entry in entries) {
                result.Add(new PostMoodServiceModel
                {
                    Id = entry.Id,
                    Timestamp = new DateTimeOffset(entry.Timestamp).ToUnixTimeSeconds(),
                    LocalTimestamp = new DateTimeOffset(entry.Timestamp.ToLocalTime()).ToUnixTimeSeconds(),
                    Activation = GetOldSurveyScaleValue(entry
                                                        ?.SurveyItemMeasurement
                                                        ?.FirstOrDefault(x => x.HardcodedQuestionId == (int)SurveyHardcodedEnumeration.Activation)?.Answer ?? 0),
                    Pleasance = GetOldSurveyScaleValue(entry
                                                        ?.SurveyItemMeasurement
                                                        ?.FirstOrDefault(x => x.HardcodedQuestionId == (int)SurveyHardcodedEnumeration.Pleasance)?.Answer ?? 0),
                    DeviceId = ""
                });
            }

            return result;
        }

        public void SetIsUploadedToServerForSurveys(List<PostMoodServiceModel> surveys) {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            foreach (var item in surveys) {
                var toUpdate = context.Get<SurveyMeasurement>(x => x.Id == item.Id);
                toUpdate.IsUploadedToServer = true;
                context.Update(toUpdate);
            } 
        }

        private int GetOldSurveyScaleValue(int newScaleValue) {

            if (newScaleValue < 33) {
                return 0;
            } 
            if (newScaleValue < 66) {
                return 1;
            }
            return 2;
        }

        public List<SurveyMeasurement> GetSurveyMeasurements() {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            return context.GetAllWithChildren<SurveyMeasurement>();
        }
    }
}
