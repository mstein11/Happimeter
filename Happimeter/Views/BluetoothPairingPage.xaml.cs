using System;
using System.Collections.Generic;
using Happimeter.ViewModels.Forms;
using Xamarin.Forms;

namespace Happimeter.Views
{
    public partial class BluetoothPairingPage : ContentPage
    {
        public BluetoothPairingPage()
        {
            Resources = App.ResourceDict;
            InitializeComponent();
            BindingContext = new BluetoothPairingPageViewModel();
        }

        void Handle_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            var vm = BindingContext as BluetoothPairingPageViewModel;
            if (vm != null)
            {
                vm.OnItemSelected(sender, e);
            }
        }
    }
}
