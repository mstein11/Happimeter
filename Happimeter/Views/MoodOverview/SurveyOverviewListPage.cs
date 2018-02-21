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

        void Handle_Pleasance_Clicked(object sender, System.EventArgs e)
        {
            var vm = (SurveyOverviewViewModel)BindingContext;
            vm.Initialize(Helpers.SurveyHardcodedEnumeration.Pleasance);
        }

        void Handle_Activation_Clicked(object sender, System.EventArgs e)
        {
            var vm = (SurveyOverviewViewModel)BindingContext;
            vm.Initialize(Helpers.SurveyHardcodedEnumeration.Activation);
        }
    }
}
