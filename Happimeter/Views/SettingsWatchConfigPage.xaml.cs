using System;
using System.Collections.Generic;
using Happimeter.ViewModels.Forms;
using Xamarin.Forms;

namespace Happimeter.Views
{
    public partial class SettingsWatchConfigPage : ContentPage
    {
        public SettingsPageWatchConfigViewModel ViewModel { get; set; }

        public SettingsWatchConfigPage()
        {
            Resources = App.ResourceDict;
            InitializeComponent();
            ViewModel = new SettingsPageWatchConfigViewModel();
            BindingContext = ViewModel;
        }
    }
}
