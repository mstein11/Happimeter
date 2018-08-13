using System;
namespace Happimeter.ViewModels.Forms
{
    public class SurveyItemViewModel : BaseViewModel
    {
        private double _answer;
        public double Answer
        {
            get => _answer;
            set
            {
                SetProperty(ref _answer, value);
                var answerDisplay = (int)(value * 100 / (100 / 3));
                if (answerDisplay == 3)
                {
                    answerDisplay = 2;
                }
                AnswerDisplay = answerDisplay;
            }
        }

        private int _answerDisplay;
        public int AnswerDisplay
        {
            get => _answerDisplay;
            set => SetProperty(ref _answerDisplay, value);
        }

        private string _question;
        public string Question
        {
            get => _question;
            set => SetProperty(ref _question, value);
        }

        private string _questionShort;
        public string QuestionShort
        {
            get => _questionShort;
            set => SetProperty(ref _questionShort, value);
        }

        private int _questionId;
        public int QuestionId
        {
            get => _questionId;
            set => SetProperty(ref _questionId, value);
        }
    }
}
