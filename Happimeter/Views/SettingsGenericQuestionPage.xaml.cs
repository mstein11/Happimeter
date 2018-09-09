using System;
using System.Collections.Generic;
using Happimeter.ViewModels.Forms;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;
using Happimeter.Interfaces;
using Happimeter.Core.Helper;

namespace Happimeter.Views
{
    public partial class SettingsGenericQuestionPage : ContentPage
    {
        public SettingsGenericQuestionPageViewModel ViewModel { get; set; }
        public SettingsGenericQuestionPage()
        {
            Resources = App.ResourceDict;
            InitializeComponent();
            ViewModel = new SettingsGenericQuestionPageViewModel();
            BindingContext = ViewModel;

            SetupQuestionContainer();
        }

        private void SetupQuestionContainer()
        {
            foreach (var item in ViewModel.GenericQuestions)
            {
                var row = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    Padding = new Thickness(0),
                };

                var nameLabel = new Label
                {
                    Text = item.Name,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                };

                var toggle = new Switch
                {
                    HorizontalOptions = LayoutOptions.End,
                };
                toggle.BindingContext = item;
                toggle.SetBinding(Switch.IsToggledProperty, new Binding(nameof(GenericQuestionViewModel.IsActivated)));

                toggle.Toggled += (object sender, ToggledEventArgs e) =>
                {
                    ServiceLocator.Instance.Get<IGenericQuestionService>().ToggleGenericQuestionActivation(item.Id, e.Value);
                    Console.WriteLine("Switch.Toggled event sent");
                };

                row.Children.Add(nameLabel);
                row.Children.Add(toggle);
                QuestionContainer.Children.Add(row);
            }
        }
    }
}
