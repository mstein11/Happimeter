using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Happimeter.ViewModels.Forms;

namespace Happimeter.Views
{
    public partial class SettingsDebugPage : ContentPage
    {
        public SettingsDebugPage()
        {
            Resources = App.ResourceDict;
            InitializeComponent();
            BindingContext = new SettingsDebugPageViewModel();
        }
    }
}
