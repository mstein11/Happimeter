using System;
using System.Collections.Generic;
using Happimeter.ViewModels.Forms;
using Xamarin.Forms;

namespace Happimeter.Views
{
    public partial class BluetoothMainPage : ContentPage
    {
        public BluetoothMainPage()
        {
            InitializeComponent();
            BindingContext = new BluetoothMainPageViewModel();
        }

        void ListItems_Refreshing(object sender, EventArgs e)
        {
            var vm = (BluetoothMainPageViewModel)BindingContext;
            vm.RefreshData();
            ListView.EndRefresh();
        }
    }
}
