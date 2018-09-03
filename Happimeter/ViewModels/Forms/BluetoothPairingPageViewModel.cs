using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;
using Happimeter.Models;
using Happimeter.Services;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Happimeter.ViewModels.Forms
{
    public class BluetoothPairingPageViewModel : BaseViewModel
    {
        public BluetoothPairingPageViewModel()
        {
            Items = new ObservableCollection<BluetoothPairingItemViewModel>();

            StartScanButtonText = "Scan for Devices";
            StartScanButtonIsEnabled = true;

            StartScanCommand = new Command(() =>
            {
                App.BluetoothAlertIfNeeded();
                if (!StartScanButtonIsEnabled)
                {
                    return;
                }
                StartScanButtonText = "Scanning...";
                StartScanButtonIsEnabled = false;
                var btService = ServiceLocator.Instance.Get<IBluetoothService>();
                Items.Clear();

                var obs = btService.StartScan();

                obs
                    .Where(x => x.Device?.Name?.Contains("Happimeter") ?? false)
                    .Finally(() =>
                    {
                        StartScanButtonIsEnabled = true;
                        StartScanButtonText = "Scan for Devices";
                    })
                   .Subscribe(result =>
                    {
                        if (Items.Select(x => x.Name).Contains(result.Device.Name))
                        {
                            var toUpdate = Items.FirstOrDefault(x => x.Name == result.Device.Name);
                            toUpdate.UpdateModel(BluetoothDevice.Create(result.Device, result.AdvertisementData.ServiceUuids));
                        }
                        else
                        {
                            Items.Add(new BluetoothPairingItemViewModel(BluetoothDevice.Create(result.Device, result.AdvertisementData.ServiceUuids)));
                        }

                    });
            });
        }

        private string _startScanButtonText;
        public string StartScanButtonText
        {
            get => _startScanButtonText;
            set => SetProperty(ref _startScanButtonText, value);

        }

        private bool _startScanButtonIsEnabled;
        public bool StartScanButtonIsEnabled
        {
            get => _startScanButtonIsEnabled;
            set => SetProperty(ref _startScanButtonIsEnabled, value);

        }

        public Command StartScanCommand { get; set; }

        public ObservableCollection<BluetoothPairingItemViewModel> Items { get; set; }

        public void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var viewModel = (e.SelectedItem as BluetoothPairingItemViewModel);
            if (viewModel == null)
            {
                return;
            }
            if (viewModel.IsUnavailable)
            {
                //show indication for 2 seconds
                (e.SelectedItem as BluetoothPairingItemViewModel).ShowIndication = true;
                (e.SelectedItem as BluetoothPairingItemViewModel).IndicationText = "Not possible";
                Timer timer = null;
                timer = new Timer((obj) =>
                {
                    (e.SelectedItem as BluetoothPairingItemViewModel).ShowIndication = false;
                    timer.Dispose();
                }, null, 2000, System.Threading.Timeout.Infinite);
                return;
            }
            var selectedItem = (e.SelectedItem as BluetoothPairingItemViewModel)?.Device as AndroidWatch ?? null;
            if (selectedItem == null)
            {


                if ((e.SelectedItem as BluetoothPairingItemViewModel) != null)
                {
                    //show indication for 2 seconds
                    (e.SelectedItem as BluetoothPairingItemViewModel).ShowIndication = true;
                    (e.SelectedItem as BluetoothPairingItemViewModel).IndicationText = "Wrong Device";
                    Timer timer = null;
                    timer = new Timer((obj) =>
                    {
                        (e.SelectedItem as BluetoothPairingItemViewModel).ShowIndication = false;
                        timer.Dispose();
                    }, null, 2000, System.Threading.Timeout.Infinite);

                }
                return;
            }
            //var _btService = ServiceLocator.Instance.Get<IBluetoothService1>();
            //_btService.PairDevice(selectedItem);
            selectedItem.Connect();

            selectedItem.OnConnectingStateChanged += (sender1, e1) =>
            {
                var state = sender1 as AndroidWatchConnectingStates?;
                if (state == null)
                {
                    return;
                }
                if (state == AndroidWatchConnectingStates.Complete)
                {
                    OnPairedDevice?.Invoke(this, null);
                }

                (e.SelectedItem as BluetoothPairingItemViewModel).ShowIndicationForState(state.Value);
            };
        }

        public event EventHandler OnPairedDevice;

        private void ReceiveConnectingEvent(object sender, EventArgs e)
        {

        }
    }
}
