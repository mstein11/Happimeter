using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Happimeter.Views
{
    public partial class InitializeSurveyView : ContentPage
    {
        public InitializeSurveyView()
        {
            InitializeComponent();

            Title = "Survey";
        }

        void Handle_Clicked(object sender, System.EventArgs e)
        {
            StartSurveyClickedEvent?.Invoke(this, null);
        }
        public event EventHandler StartSurveyClickedEvent;
    }
}
