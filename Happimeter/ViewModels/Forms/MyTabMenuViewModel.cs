using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
namespace Happimeter.ViewModels.Forms
{
    public class MyTabMenuViewModel : BaseViewModel
    {
        public MyTabMenuViewModel()
        {
        }

        private ObservableCollection<TabMenuItemViewModel> _items;
        public ObservableCollection<TabMenuItemViewModel> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }
    }

    public class TabMenuItemViewModel : BaseViewModel
    {
        public TabMenuItemViewModel()
        {
            OnTabChangedCommand = new Command<int>((index) =>
            {
                Debug.WriteLine(index);
            });
        }
        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _text;
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        private Command<int> _onTabChangedCommand;
        public Command<int> OnTabChangedCommand
        {
            get => _onTabChangedCommand;
            set => SetProperty(ref _onTabChangedCommand, value);
        }
    }
}
