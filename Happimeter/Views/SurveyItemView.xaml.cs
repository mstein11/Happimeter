using System;
using System.Collections.Generic;
using Happimeter.ViewModels.Forms;
using Xamarin.Forms;

namespace Happimeter.Views
{
    public partial class SurveyItemView : ContentView
    {
        public SurveyItemView(SurveyItemViewModel viewModel, bool grayBackground = false)
        {
            BindingContext = viewModel;
            InitializeComponent();

            if (grayBackground) {
                Container.BackgroundColor = Color.LightGray;    
            }
        }
    }
}
