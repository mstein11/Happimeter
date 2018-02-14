using System;
using System.Collections.Generic;
using System.Linq;
using Happimeter.Core.Database;

namespace Happimeter.Watch.Droid.ViewModels
{
    public class SurveyViewModel
    {
        public SurveyViewModel()
        {
        }

        public List<SurveyFragmentViewModel> Questions = new List<SurveyFragmentViewModel>();

        public SurveyFragmentViewModel GetCurrentQuestion() => Questions.FirstOrDefault(x => !x.IsAnswered);
        public int GetCurrentQuestionPosition() => Questions.FindIndex(x => !x.IsAnswered);

        public SurveyMeasurement GetDataModel() {

            var answers = new List<SurveyItemMeasurement>();

            foreach (var answer in Questions) {
                answers.Add(new SurveyItemMeasurement {
                    Answer = answer.Answer ?? 0,
                    AnswerDisplay = answer.AnswerDisplay,
                    Question = answer.Question,
                    HardcodedQuestionId = answer.HardcodedId
                });
            }

            var measurement = new SurveyMeasurement()
            {
                Timestamp = DateTime.UtcNow,
                SurveyItemMeasurement = answers
            };

            return measurement;
        }
    }
}
