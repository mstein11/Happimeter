﻿using System;
using System.Collections.Generic;
using Happimeter.Interfaces;
using Happimeter.ViewModels.Forms;
using Xamarin.Forms;
using Microcharts.Forms;
using Happimeter.Views.Converters;
using Happimeter.Core.Helpers;
using System.Linq;
using System.Threading.Tasks;

namespace Happimeter.Views.MoodOverview
{
    public partial class SurveyOverviewListPage : ContentPage
    {
        public SurveyOverviewListPage()
        {
            Resources = App.ResourceDict;
            InitializeComponent();

            BindingContext = new SurveyOverviewViewModel();
        }

        void Handle_Pleasance_Clicked(object sender, System.EventArgs e)
        {
            var vm = (SurveyOverviewViewModel)BindingContext;
            vm.Initialize((int)SurveyHardcodedEnumeration.Pleasance);
        }

        void Handle_Activation_Clicked(object sender, System.EventArgs e)
        {
            var vm = (SurveyOverviewViewModel)BindingContext;
            vm.Initialize((int)SurveyHardcodedEnumeration.Activation);
        }

        void ListItems_Refreshing(object sender, EventArgs e)
        {
            var vm = (SurveyOverviewViewModel)BindingContext;
            vm.RefreshData();
            SurveyListView.EndRefresh();
        }

        void Handle_ItemAppearing(object sender, Xamarin.Forms.ItemVisibilityEventArgs e)
        {
            var vm = (SurveyOverviewViewModel)BindingContext;
            if (!vm.Items.Any() || SurveyListView.IsRefreshing)
            {
                return;
            }

            if (((SurveyOverviewItemViewModel)e.Item).Date == vm.Items.LastOrDefault().Date)
            {
                Task.Factory.StartNew(() =>
                {
                    vm.LoadMoreData();
                });
            }
        }

        void Handle_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            var selectedItem = ((SurveyOverviewItemViewModel)e.SelectedItem);
            ItemSelectedEvent?.Invoke(selectedItem, null);
            ((ListView)sender).SelectedItem = null;
        }
        public event EventHandler ItemSelectedEvent;
    }
}
