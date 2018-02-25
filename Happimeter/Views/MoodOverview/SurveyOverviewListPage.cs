﻿using System;
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
            BindingContext = new SurveyOverviewViewModel();
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

        void ListItems_Refreshing(object sender, EventArgs e) {
            var vm = (SurveyOverviewViewModel)BindingContext;
            vm.RefreshData();
            SurveyListView.EndRefresh();
        }
    }
}