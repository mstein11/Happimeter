using System;
using System.Collections.Generic;

namespace Happimeter.ViewModels.Forms
{
    public class SurveyViewModel : BaseViewModel
    {
        public SurveyViewModel()
        {
        }


        private List<SurveyItemViewModel> _surveyItems = new List<SurveyItemViewModel>();
        public List<SurveyItemViewModel> SurveyItems
        {
            get => _surveyItems;
            set => SetProperty(ref _surveyItems, value);
        }
    }
}
