using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Happimeter.Interfaces;
using Happimeter.Models;

namespace Happimeter.ViewModels.Forms
{
    public class BluetoothPairingPageViewModel
    {
        public BluetoothPairingPageViewModel()
        {
            Items = new ObservableCollection<BluetoothDevice>();
            StartScanCommand = new Command(() =>
            {
                var btService = ServiceLocator.Instance.Get<IBluetoothService>();
                Items.Clear();
                var obs = btService.StartScan();
                obs.Subscribe(result => {
                    Items.Add(BluetoothDevice.Create(result.Device));
                });
            });
        }

        public Command StartScanCommand { get; set; }

        public ObservableCollection<BluetoothDevice> Items { get; set; }
    }
}
