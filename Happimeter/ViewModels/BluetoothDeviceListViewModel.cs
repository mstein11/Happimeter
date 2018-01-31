using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Happimeter.Models;

namespace Happimeter.ViewModels
{
    public class BluetoothDeviceListViewModel : BaseViewModel
    {
        public ObservableCollection<BluetoothDevice> Items { get; set; }
        public Command LoadItemsCommand { get; set; }
        public Command AddItemCommand { get; set; }

        public BluetoothDeviceListViewModel()
        {
            Title = "BL Devices";
            Items = new System.Collections.ObjectModel.ObservableCollection<BluetoothDevice>();
            //LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            AddItemCommand = new Command<BluetoothDevice>(async (BluetoothDevice item) => await AddItem(item));
        }

        public void AddDevice(BluetoothDevice device) {

            try {
                if (Items.Select(x => x.Device.Uuid).All(x => x != device.Device.Uuid)) {
                    if (IsBusy)
                    {
                        return;
                    }
                    IsBusy = true;
                    Items.Add(device);                    
                }
                //Items.Clear();
            } catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }


        async Task AddItem(BluetoothDevice item)
        {
            Items.Add(item);
            //await DataStore.AddItemAsync(item);
        }
    }
}
