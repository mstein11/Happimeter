using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Happimeter.Interfaces;
using Happimeter.Models;
using Xamarin.Forms;

namespace Happimeter.ViewModels.Forms
{
    public class BluetoothPairingPageViewModel
    {
        public BluetoothPairingPageViewModel()
        {
            Items = new ObservableCollection<BluetoothPairingItemViewModel>();
            StartScanCommand = new Command(() =>
            {
                var btService = ServiceLocator.Instance.Get<IBluetoothService>();
                Items.Clear();
                var obs = btService.StartScan();
                obs.Subscribe(result => {
                    Items.Add(new BluetoothPairingItemViewModel(BluetoothDevice.Create(result.Device)));
                });
            });
        }

        public Command StartScanCommand { get; set; }

        public ObservableCollection<BluetoothPairingItemViewModel> Items { get; set; }

        public void OnItemSelected(object sender, SelectedItemChangedEventArgs e) {
            var selectedItem = (e.SelectedItem as BluetoothPairingItemViewModel)?.Device as AndroidWatch ?? null;
            if (selectedItem == null) {


                if ((e.SelectedItem as BluetoothPairingItemViewModel) != null) {
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
            var _btService = ServiceLocator.Instance.Get<IBluetoothService>();
            _btService.PairDevice(selectedItem);

            selectedItem.OnConnectingStateChanged += (sender1, e1) =>
            {
                var state = sender1 as AndroidWatchConnectingStates?;
                if (state == null)
                {
                    return;
                }
                if (state == AndroidWatchConnectingStates.Complete) {
                    OnPairedDevice?.Invoke(this, null);    
                }

                (e.SelectedItem as BluetoothPairingItemViewModel).ShowIndicationForState(state.Value);
            };


        }

        public event EventHandler OnPairedDevice;

        private void ReceiveConnectingEvent(object sender, EventArgs e) {
            
        }
    }
}
