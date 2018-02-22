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
            InitializeComponent();
            BindingContext = new BluetoothPairingPageViewModel();
        }
    }
}
