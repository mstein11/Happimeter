using System;
using System.Collections.Generic;
using Happimeter.Interfaces;
using Happimeter.ViewModels.Forms;
using Xamarin.Forms;
using Microcharts.Forms;

namespace Happimeter.Views.MoodOverview
{
    public partial class SurveyOverviewListPage : ContentPage
    {
        public SurveyOverviewListPage()
        {
            InitializeComponent();

            var surveyData = ServiceLocator.Instance.Get<IMeasurementService>().GetSurveyData();
            BindingContext = new SurveyOverviewViewModel(surveyData);
        }
    }
}
