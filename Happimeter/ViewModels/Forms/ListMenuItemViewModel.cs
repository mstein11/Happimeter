using System;
using Xamarin.Forms;

namespace Happimeter.ViewModels.Forms
{
    public class ListMenuItemViewModel : BaseViewModel
    {
        public ListMenuItemViewModel()
        {
        }

        private string _itemTitle;
        public string ItemTitle
        {
            get => _itemTitle;
            set => SetProperty(ref _itemTitle, value);
        }

        private string _iconText;
        public string IconText
        {
            get => _iconText;
            set => SetProperty(ref _iconText, value);
        }

        private Color _iconBackgroundColor = Color.Gray;
        public Color IconBackgroundColor
        {
            get => _iconBackgroundColor;
            set => SetProperty(ref _iconBackgroundColor, value);
        }

        private Color _iconTextColor = Color.White;
        public Color IconTextColor
        {
            get => _iconTextColor;
            set => SetProperty(ref _iconTextColor, value);
        }

        public Command OnClickedCommand { get; set; }

    }
}
