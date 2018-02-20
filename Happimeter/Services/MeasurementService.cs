using System;
using System.Collections.Generic;
using Happimeter.Core.Database;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Interfaces;
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
                Answer = .5
            };
            var question2 = new SurveyItemViewModel
            {
                Question = "How Active do you feel?",
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

            foreach (var measurement in message.SurveyMeasurements)
            {
                measurement.IdFromWatch = measurement.Id;
                measurement.Id = 0;
                context.AddGraph(measurement);
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
                    Question = item.Question,
                    AnswerDisplay = item.AnswerDisplay,
                });  
            }

            context.AddGraph(surveyMeasurement);
        }

        public List<SurveyMeasurement> GetSurveyMeasurements() {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            return context.GetAllWithChildren<SurveyMeasurement>();
        }
    }
}
