using Foundation;
using System;
using UIKit;
using Happimeter.ViewModels;
using System.Collections.Specialized;
using Happimeter.Interfaces;
using Happimeter.Models;
using System.Diagnostics;

namespace Happimeter.iOS
{
    public partial class BluetoothDeviceListViewController : UITableViewController
    {

        private IBluetoothService _btService;
        UIRefreshControl refreshControl;

        public BluetoothDeviceListViewModel ViewModel { get; set; }


        public BluetoothDeviceListViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewModel = new BluetoothDeviceListViewModel();

            // Setup UITableView.
            refreshControl = new UIRefreshControl();
            refreshControl.ValueChanged += RefreshControl_ValueChanged;
            TableView.Add(refreshControl);
            TableView.Source = new BluetoothDeviceDataSource(ViewModel);

            Title = ViewModel.Title;

            ViewModel.PropertyChanged += IsBusy_PropertyChanged;
            ViewModel.Items.CollectionChanged += Items_CollectionChanged;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (_btService == null) {
                _btService = ServiceLocator.Instance.Get<IBluetoothService>();
                _btService.StartScan().Subscribe(x => {
                    var vm = BluetoothDevice.Create(x.Device);
                    //var vm = new BluetoothDevice();
                    vm.Name = x.Device.Name;
                    vm.Description = x.AdvertisementData.IsConnectable.ToString();
                    vm.Device = x.Device;
                    this.ViewModel.AddDevice(vm);

                });
            }
                
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var item = ViewModel.Items[indexPath.Row];
            _btService = ServiceLocator.Instance.Get<IBluetoothService>();
            _btService.PairDevice(item.Device);
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            
                var controller = segue.DestinationViewController as BtDeviceDetailViewController;
                var indexPath = TableView.IndexPathForCell(sender as UITableViewCell);
                var item = ViewModel.Items[indexPath.Row];

                controller.ViewModel = item;

        }

        void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            if (!ViewModel.IsBusy && refreshControl.Refreshing)
                ViewModel.LoadItemsCommand.Execute(null);
        }

        void IsBusy_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var propertyName = e.PropertyName;
            switch (propertyName)
            {
                case nameof(ViewModel.IsBusy):
                    {
                        InvokeOnMainThread(() =>
                        {
                            if (ViewModel.IsBusy && !refreshControl.Refreshing)
                                refreshControl.BeginRefreshing();
                            else if (!ViewModel.IsBusy)
                                refreshControl.EndRefreshing();
                        });
                    }
                    break;
            }
        }

        void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InvokeOnMainThread(() => TableView.ReloadData());
        }
    }

    class BluetoothDeviceDataSource : UITableViewSource
    {
        static readonly NSString CELL_IDENTIFIER = new NSString("ITEM_CELL_BL");

        BluetoothDeviceListViewModel viewModel;

        public BluetoothDeviceDataSource(BluetoothDeviceListViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public override nint RowsInSection(UITableView tableview, nint section) => viewModel.Items.Count;
        public override nint NumberOfSections(UITableView tableView) => 1;

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(CELL_IDENTIFIER, indexPath);

            var item = viewModel.Items[indexPath.Row];
            cell.TextLabel.Text = item.Device.Uuid.ToString();
            cell.DetailTextLabel.Text = item.Name;
            cell.LayoutMargins = UIEdgeInsets.Zero;

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {

        }
    }
}