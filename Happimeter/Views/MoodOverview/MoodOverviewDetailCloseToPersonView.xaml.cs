using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Happimeter.ViewModels.Forms;

namespace Happimeter.Views.MoodOverview
{
    public partial class MoodOverviewDetailCloseToPersonView : ContentView
    {
        public MoodOverviewDetailCloseToPersonView(SurveyOverviewDetailCloseToPersonViewModel viewModel)
        {
            BindingContext = viewModel;
            InitializeComponent();


        }

        public async void OnApearing()
        {
            await ScrollView.ScrollToAsync(chart, ScrollToPosition.End, true);
            await ScrollView.ScrollToAsync(chart, ScrollToPosition.Start, true);
        }
    }
}
