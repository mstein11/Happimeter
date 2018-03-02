using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;

namespace Happimeter
{
    public class ItemsViewModel : BaseViewModel
    {
        public ObservableCollection<Item> Items { get; set; }
        public Command LoadItemsCommand { get; set; }
        public Command AddItemCommand { get; set; }

        public ItemsViewModel()
        {
            Title = "Browse";
            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            AddItemCommand = new Command<Item>(async (Item item) => await AddItem(item));
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                var measurements = ServiceLocator.Instance.Get<IMeasurementService>().GetSurveyMeasurements().OrderByDescending(x => x.Timestamp);
                var newItems = new List<Item>();
                foreach (var measurement in measurements) {
                    var item = new Item
                    {
                        Text = measurement.Timestamp.ToString(),
                        Description = string.Concat(measurement.SurveyItemMeasurement.Select((x, index) => $"{index}: {x.Answer}. "))
                    };
                    newItems.Add(item);
                }
                foreach (var item in newItems)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task AddItem(Item item)
        {
            Items.Add(item);
            await DataStore.AddItemAsync(item);
        }
    }
}
