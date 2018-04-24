using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Happimeter.ViewModels.Forms;
using Plugin.BluetoothLE;

namespace Happimeter.Views.MoodOverview
{
    public partial class SurveyOverviewDetailPage : ContentPage
    {

        public SurveyOverviewDetailPage(DateTime forDay)
        {
            Resources = App.ResourceDict;
            var viewModel = new SurveyOverviewDetailPageViewModel(forDay);
            BindingContext = viewModel;
            InitializeComponent();
            foreach (var item in viewModel.CloseToPersons)
            {
                PersonContainer.Children.Add(new MoodOverviewDetailCloseToPersonView(item));
            }

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            foreach (var elem in PersonContainer.Children)
            {
                var view = elem as MoodOverviewDetailCloseToPersonView;
                if (view != null)
                {
                    view.OnApearing();
                }
            }
        }
    }
}
