using System;
namespace Happimeter.Watch.Droid.ViewModels
{
    public class SurveyFragmentViewModel : BaseViewModel
    {
        public SurveyFragmentViewModel(SurveyViewModel parentViewModel)
        {
            ParentViewModel = parentViewModel;
        }

        private string _question = null;
        public string Question
        {
            get => _question;
            set => SetProperty(ref _question, value);
        }

        private string _questionShort = null;
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
            set
            {
                SetProperty(ref _answer, value);
                var display = value != null ? (int)(value / (100 / 3)) : 0;
                if (display == 3)
                {
                    display = 2;
                }
                AnswerDisplay = display;
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

        private SurveyViewModel _parentViewModel;
        public SurveyViewModel ParentViewModel
        {
            get => _parentViewModel;
            set => SetProperty(ref _parentViewModel, value);
        }

    }
}
