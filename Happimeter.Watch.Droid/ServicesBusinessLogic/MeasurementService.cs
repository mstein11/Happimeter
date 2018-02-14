using System;
using System.Collections.Generic;
using Happimeter.Core.Database;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.ViewModels;

namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
    public class MeasurementService : IMeasurementService
    {
        public MeasurementService()
        {
        }

        public void AddSurveyMeasurement(SurveyMeasurement measurement) {
            var dbContext = ServiceLocator.Instance.Get<IDatabaseContext>();
            dbContext.AddGraph(measurement);
        }

        public SurveyViewModel GetSurveyQuestions() {
            var questions = new SurveyViewModel();
            var question1 = new SurveyFragmentViewModel
            {
                Question = "How Pleasant do you feel?",
                IsHardcodedQuestion = true,
                HardcodedId = 1
            };
            var question2 = new SurveyFragmentViewModel
            {
                Question = "How Active do you feel?",
                IsHardcodedQuestion = true,
                HardcodedId = 2
            };
            var question3 = new SurveyFragmentViewModel
            {
                Question = "How Stressed do you feel?",
                IsHardcodedQuestion = true,
            };
            questions.Questions.Add(question1);
            questions.Questions.Add(question2);
            questions.Questions.Add(question3);

            return questions;
        }

        public DataExchangeMessage GetMeasurementsForDataTransfer() 
        {
            var moods = ServiceLocator.Instance.Get<IDatabaseContext>().GetAllWithChildren<SurveyMeasurement>();
            //remove self referencing loop so that we can serialize it
            moods.ForEach(x => x.SurveyItemMeasurement.ForEach(y => y.SurveyMeasurement = null));

            return new DataExchangeMessage { SurveyMeasurements = moods };
        }
    }
}
