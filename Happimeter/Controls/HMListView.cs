using System;
using Xamarin.Forms;
namespace Happimeter.Controls
{
    public class HMListView : ListView
    {
        public HMListView()
        {
        }

        public static readonly BindableProperty RowsSelectableProperty =
            BindableProperty.Create(nameof(RowsSelectable), typeof(bool), typeof(HMListView), true);

        public bool RowsSelectable
        {
            get
            {
                return (bool)GetValue(RowsSelectableProperty);
            }
            set
            {
                SetValue(RowsSelectableProperty, value);
            }
        }
    }
}
