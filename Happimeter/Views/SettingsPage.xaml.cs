using System;
using System.Collections.Generic;
using Happimeter.ViewModels.Forms;
using Xamarin.Forms;

namespace Happimeter.Views
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            Resources = App.ResourceDict;
            InitializeComponent();
            ViewModel = new SettingsPageViewModel();
            BindingContext = ViewModel;
        }

        public SettingsPageViewModel ViewModel { get; set; }
    }
}
