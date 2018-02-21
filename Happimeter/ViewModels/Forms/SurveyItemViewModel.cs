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
                var answerDisplay = (int)(value * 100 / (100 / 9)) + 1;
                if (answerDisplay == 10) {
                    answerDisplay = 9;
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

        private int _hardcodedId;
        public int HardcodedId 
        {
            get => _hardcodedId;
            set => SetProperty(ref _hardcodedId, value);
        }
    }
}
