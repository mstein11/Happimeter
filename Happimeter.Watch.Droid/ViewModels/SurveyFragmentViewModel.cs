using System;
namespace Happimeter.Watch.Droid.ViewModels
{
    public class SurveyFragmentViewModel : BaseViewModel
    {
        public SurveyFragmentViewModel()
        {
        }

        private string _question = null;
        public string Question
        {
            get => _question;
            set => SetProperty(ref _question, value);
        }

        private int? _hardcodedId = null;
        public int? HardcodedId
        {
            get => _hardcodedId;
            set => SetProperty(ref _hardcodedId, value);
        }

        private bool _isHardcodedQuestion = false;
        public bool IsHardcodedQuestion
        {
            get => _isHardcodedQuestion;
            set => SetProperty(ref _isHardcodedQuestion, value);
        }
        private int? _answer = null;
        public int? Answer
        {
            get => _answer;
            set {
                SetProperty(ref _answer, value);
                AnswerDisplay = value != null ? (int)(value / (100 / 3)) : 0;
            } 
        }

        private bool _isAnswered = false;
        public bool IsAnswered 
        {
            get => _isAnswered;
            set => SetProperty(ref _isAnswered, value);
        }

        private int _answerDisplay = 1;
        public int AnswerDisplay 
        {
            get => _answerDisplay;
            set => SetProperty(ref _answerDisplay, value);
        }


    }
}
