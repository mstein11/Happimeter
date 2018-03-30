using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Happimeter.Core.Database;
using Happimeter.Core.Helper;
using Happimeter.Interfaces;

namespace Happimeter.ViewModels.Forms
{
    public class BluetoothMainPageViewModel : BaseViewModel
    {
        public BluetoothMainPageViewModel()
        {
            Items = new ObservableCollection<BluetoothMainItemViewModel>();

            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var pairing = context
                          .Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);

            SetValuesInViewModel(pairing);

            context.WhenEntryChanged<SharedBluetoothDevicePairing>().Subscribe(x =>
            {
                foreach (var entry in x.Entites)
                {
                    var updatedPairing = (entry as SharedBluetoothDevicePairing);
                    if (updatedPairing != null && updatedPairing.IsPairingActive)
                    {
                        SetValuesInViewModel(updatedPairing);
                    }
                }
            });

            RemovePairingCommand = new Command(() =>
            {
                var btService = ServiceLocator.Instance.Get<IBluetoothService>();
                btService.RemoveAllConnections();
                var innerContext = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
                var innerPairing = context
                              .Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);

                if (innerPairing != null)
                {
                    context.Delete(innerPairing);
                }
                OnRemovedPairing?.Invoke(this, null);
            });

            ExchangeDataCommand = new Command(() =>
            {
                App.BluetoothAlertIfNeeded();
                var btService = ServiceLocator.Instance.Get<IBluetoothService>();
                btService.ExchangeData();
            });

            RefreshData();

            var btServiceForUpdate = ServiceLocator.Instance.Get<IBluetoothService>();
            btServiceForUpdate.DataExchangeStatusUpdate += (sender, e) =>
            {
                switch (e.EventType)
                {
                    case Events.AndroidWatchExchangeDataStates.SearchingForDevice:
                        DisplayIndication("Searching for device");
                        break;
                    case Events.AndroidWatchExchangeDataStates.DeviceConnected:
                        DisplayIndication("Device Connected");
                        break;
                    case Events.AndroidWatchExchangeDataStates.CharacteristicDiscovered:
                        DisplayIndication("Characteristic discovered");
                        break;
                    case Events.AndroidWatchExchangeDataStates.DidWrite:
                        DisplayIndication("Did Write.");
                        break;
                    case Events.AndroidWatchExchangeDataStates.ReadUpdate:
                        DisplayIndication("Exchanging Data...", e.BytesRead);
                        break;
                    case Events.AndroidWatchExchangeDataStates.Complete:
                        DisplayIndication("Data Exchange Complete", e.BytesRead, 2000);
                        break;
                    case Events.AndroidWatchExchangeDataStates.DeviceNotFound:
                        DisplayIndication("Device could not be found", e.BytesRead, 2000);
                        break;
                    case Events.AndroidWatchExchangeDataStates.CouldNotConnect:
                        DisplayIndication("Could not connect to device", e.BytesRead, 2000);
                        break;
                    case Events.AndroidWatchExchangeDataStates.CouldNotDiscoverCharacteristic:
                        DisplayIndication("Could not discover characteristic", e.BytesRead, 2000);
                        break;
                    case Events.AndroidWatchExchangeDataStates.ErrorOnExchange:
                        DisplayIndication("Error while exchanging data", e.BytesRead, 2000);
                        break;
                }
            };

        }

        private DateTime _synchronizedAt;
        public DateTime SynchronizedAt
        {
            get => _synchronizedAt;
            set => SetProperty(ref _synchronizedAt, value);
        }

        private DateTime _pairedAt;
        public DateTime PairedAt
        {
            get => _pairedAt;
            set => SetProperty(ref _pairedAt, value);
        }

        private bool _dataExchangeStatusIsVisible;
        public bool DataExchangeStatusIsVisible
        {
            get => _dataExchangeStatusIsVisible;
            set => SetProperty(ref _dataExchangeStatusIsVisible, value);
        }

        private string _dataExchangeStatus;
        public string DataExchangeStatus
        {
            get => _dataExchangeStatus;
            set => SetProperty(ref _dataExchangeStatus, value);
        }

        private bool _dataExchangeProgressIsVisible;
        public bool DataExchangeProgressIsVisible
        {
            get => _dataExchangeProgressIsVisible;
            set => SetProperty(ref _dataExchangeProgressIsVisible, value);
        }

        private int _dataExchangeProgress;
        public int DataExchangeProgress
        {
            get => _dataExchangeProgress;
            set => SetProperty(ref _dataExchangeProgress, value);
        }

        public ObservableCollection<BluetoothMainItemViewModel> Items { get; set; }

        public Command RemovePairingCommand { get; set; }

        public Command ExchangeDataCommand { get; set; }


        public event EventHandler OnRemovedPairing;


        public void RefreshData()
        {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            //todo: use method that is specific for sensormeasurements and adjust the ordering in this method
            //var measurements = context.GetAllWithChildren<SensorMeasurement>().OrderByDescending(x => x.Timestamp).Take(100);
            var measurements = context.GetSensorMeasurements(0, 100, true);
            Items.Clear();
            foreach (var measurement in measurements)
            {
                Items.Add(new BluetoothMainItemViewModel(measurement));
            }
        }

        public void LoadMoreData()
        {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var measurements = context.GetSensorMeasurements(Items.Count, 100, true);
            //var measurements = context.GetAllWithChildren<SensorMeasurement>().OrderByDescending(x => x.Timestamp).Skip(Items.Count()).Take(100);
            foreach (var measurement in measurements)
            {
                Items.Add(new BluetoothMainItemViewModel(measurement));
            }
        }

        private void SetValuesInViewModel(SharedBluetoothDevicePairing pairing)
        {
            var lastExchange = pairing?.LastDataSync;
            if (lastExchange != null)
            {
                SynchronizedAt = lastExchange.Value.ToLocalTime();
            }

            var pairingTime = pairing?.PairedAt;
            if (pairingTime != null)
            {
                PairedAt = pairingTime.Value.ToLocalTime();
            }
        }

        private void DisplayIndication(string text, int? progress = null, int? milliseconds = null)
        {
            DataExchangeStatusIsVisible = true;
            DataExchangeStatus = text;
            Timer timer = null;

            if (progress != null)
            {
                DataExchangeProgressIsVisible = true;
                DataExchangeProgress = progress.Value;
            }

            if (milliseconds != null)
            {
                timer = new Timer((obj) =>
                {
                    DataExchangeStatus = null;
                    DataExchangeStatusIsVisible = false;
                    DataExchangeProgressIsVisible = false;
                    DataExchangeProgress = 0;

                    timer.Dispose();
                }, null, milliseconds.Value, System.Threading.Timeout.Infinite);
            }
        }
    }
}
