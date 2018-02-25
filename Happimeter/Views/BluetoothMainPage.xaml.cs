using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            Task.Factory.StartNew(() => {
                var vm = (BluetoothMainPageViewModel)BindingContext;
                vm.RefreshData();
                Device.BeginInvokeOnMainThread(() => {
                    ListView.EndRefresh();        
                });
            });

        }

        void Handle_ItemAppearing(object sender, Xamarin.Forms.ItemVisibilityEventArgs e)
        {
            var vm = (BluetoothMainPageViewModel)BindingContext;
            if (!vm.Items.Any() || ListView.IsRefreshing)
            {
                return;
            }

            if(((BluetoothMainItemViewModel)e.Item).TimeStamp == vm.Items.LastOrDefault().TimeStamp) {
                Task.Factory.StartNew(() =>
                {
                    vm.LoadMoreData();
                });
            }
        }
    }
}
