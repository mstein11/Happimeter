using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using Happimeter.ViewModels.Forms;

namespace Happimeter.Controls
{
    public partial class ListMenuView : ContentView
    {
        public static readonly BindableProperty ListMenuItemsProperty =
            BindableProperty.Create("ListMenuItemsProperty", typeof(List<ListMenuItemViewModel>), typeof(ListMenuView), new List<ListMenuItemViewModel>(), propertyChanged: OnItemsSourceChanged);

        public List<ListMenuItemViewModel> ListMenuItems
        {
            get
            {
                return (List<ListMenuItemViewModel>)GetValue(ListMenuItemsProperty);
            }
            set
            {
                SetValue(ListMenuItemsProperty, value);
            }
        }

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (ListMenuView)bindable;
            var items = (List<ListMenuItemViewModel>)newValue;
            view.SetupPage();
        }

        private void SetupPage()
        {
            Content.Children.Clear();
            foreach (var item in ListMenuItems)
            {
                var layout = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 5,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Padding = new Thickness(0, 5)
                };

                var recognizer = new TapGestureRecognizer();
                recognizer.Command = new Command(async () =>
                {
                    Debug.WriteLine("tabbed");
                    layout.BackgroundColor = Color.Gray;
                    item.OnClickedCommand?.Execute(null);
                    await Task.Delay(1000);
                    layout.BackgroundColor = Color.Transparent;
                });
                layout.GestureRecognizers.Add(recognizer);

                var icon = new StackLayout
                {
                    HeightRequest = 30,
                    WidthRequest = 30,
                    BackgroundColor = item.IconBackgroundColor,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                };

                var iconText = new Label
                {
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    TextColor = item.IconTextColor,
                    Text = item.IconText
                };
                icon.Children.Add(iconText);

                var text = new Label
                {
                    Text = item.ItemTitle,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                };
                var indicator = new Label
                {
                    Text = ">",
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                };
                layout.Children.Add(icon);
                layout.Children.Add(text);
                layout.Children.Add(indicator);
                Content.Children.Add(layout);
            }
        }

        public ListMenuView()
        {
            InitializeComponent();
            SetupPage();
        }
    }
}
