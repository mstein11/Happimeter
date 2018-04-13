using System;

using Xamarin.Forms;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Happimeter.ViewModels.Forms;
using System.Linq;
using Android.Views;

namespace Happimeter.Controls
{
    public class MyTabMenu : ContentView
    {
        public MyTabMenu()
        {
        }

        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create("Items", typeof(MyTabMenuViewModel), typeof(MyTabMenu), default(MyTabMenuViewModel), propertyChanged: OnItemsSourceChanged);

        public MyTabMenuViewModel ViewModel
        {
            get => (MyTabMenuViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly BindableProperty CurrentIndexProptery =
                BindableProperty.Create("CurrentIndex", typeof(int), typeof(MyTabMenu), default(int));

        public int CurrentIndex
        {
            get => (int)GetValue(CurrentIndexProptery);
            set => SetValue(CurrentIndexProptery, value);
        }

        public static readonly BindableProperty OnTabChangedCommandProperty =
            BindableProperty.Create("OnTabChangedCommand", typeof(Command<int>), typeof(MyTabMenu), default(Command<int>));

        public Command<int> OnTabChangedCommand
        {
            get => (Command<int>)GetValue(OnTabChangedCommandProperty);
            set => SetValue(OnTabChangedCommandProperty, value);
        }

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var layout = (MyTabMenu)bindable;
            layout.SetupLayout();
        }

        private void SetupLayout()
        {
            var scrollView = Content as ScrollView;
            if (scrollView == null)
            {
                scrollView = new ScrollView
                {
                    Orientation = ScrollOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                };
            }
            var children = new List<Xamarin.Forms.View>();
            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = BackgroundColor,
                Padding = new Thickness(10, 0, 10, 0)
            };

            foreach (var item in ViewModel.Items)
            {
                item.OnTabChangedCommand = new Command<int>((questionId) =>
                {
                    OnTabChangedCommand?.Execute(questionId);
                    var newActive = ViewModel.Items.FirstOrDefault(x => x.Id == questionId);
                    //first set the new tab to true, because otherwise the view resizes and this leads to undesired behavior
                    newActive.IsActive = true;
                    foreach (var innerItem in ViewModel.Items)
                    {
                        if (questionId == innerItem.Id)
                        {
                            //do not await this, awaiting it leads to buggy behavior
                            scrollView.ScrollToAsync(stackLayout.Children.FirstOrDefault(x => x.BindingContext == innerItem), ScrollToPosition.Center, true);
                        }
                        else
                        {
                            innerItem.IsActive = false;
                        }
                    }
                });
                var myTabMenuItem = new MyTabMenuItem();
                myTabMenuItem.BindingContext = item;
                myTabMenuItem.Padding = new Thickness(10, 0, 10, 0);
                stackLayout.Children.Add(
                    myTabMenuItem
                );
            }
            scrollView.Content = stackLayout;

            Content = scrollView;
        }
    }
}

