using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Happimeter.ViewModels.Forms;
using System.Diagnostics;
using System.Globalization;

namespace Happimeter.Controls
{
    public partial class MoodBar : StackLayout
    {

        public MoodBar()
        {
            InitializeComponent();
        }

        public MoodBar(MoodBarViewModel viewModel)
        {
            InitializeComponent();
            //ViewModel = viewModel;
            BindingContext = viewModel;
        }

        public void OnBarTabbed(object sender, EventArgs e)
        {
            Debug.WriteLine("Tabbed");
        }
    }

    public class BackgroundColorValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Color.FromHex("#0dd5fc");
            }
            return default(Color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
