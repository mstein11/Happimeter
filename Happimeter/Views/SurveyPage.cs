using System;
using System.Collections.Generic;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using Happimeter.ViewModels.Forms;
using Xamarin.Forms;

namespace Happimeter.Views
{
    public partial class SurveyPage : ContentPage
    {
        private SurveyViewModel ViewModel { get; set; }
        public SurveyPage()
        {
            InitializeComponent();


            var measurementService = ServiceLocator.Instance.Get<IMeasurementService>();
            var questions = measurementService.GetSurveyQuestions();

            var idx = 0;
            foreach (var question in questions.SurveyItems) {
                QuestionsContainer.Children.Add(new SurveyItemView(question, idx % 2 == 1));
                idx++;
            }
            ViewModel = questions;
        }

        void Handle_Confirm_Clicked(object sender, System.EventArgs e)
        {
            var measurementService = ServiceLocator.Instance.Get<IMeasurementService>();
            var apiService = ServiceLocator.Instance.Get<IHappimeterApiService>();
            measurementService.AddSurveyData(ViewModel);
            FinishedSurvey?.Invoke(this, null);
        }

        public event EventHandler FinishedSurvey;
    }
}
