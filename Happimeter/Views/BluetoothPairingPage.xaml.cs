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
            if (e.SelectedItem == null)
            {
                return;
            }
            var vm = BindingContext as BluetoothPairingPageViewModel;
            if (vm != null)
            {
                vm.OnItemSelected(sender, e);
                (e.SelectedItem as BluetoothPairingItemViewModel).PropertyChanged += (object innerSender, System.ComponentModel.PropertyChangedEventArgs propertyChangedE) =>
                {
                    if (propertyChangedE.PropertyName == nameof(BluetoothPairingItemViewModel.ShowIndication) && (innerSender as BluetoothPairingItemViewModel) != null && !(innerSender as BluetoothPairingItemViewModel).ShowIndication)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            ((ListView)sender).SelectedItem = null;
                        });
                    }
                };
            }
        }
    }
}
