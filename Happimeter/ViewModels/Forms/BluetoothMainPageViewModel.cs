using System;
using Happimeter.Core.Database;
using Happimeter.Interfaces;

namespace Happimeter.ViewModels.Forms
{
    public class BluetoothMainPageViewModel : BaseViewModel
    {
        public BluetoothMainPageViewModel()
        {
            var context = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var pairing = context
                          .Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);

            SetValuesInViewModel(pairing);

            context.ModelChanged += (object sender, EventArgs e) =>
            {
                var updatedPairing = (sender as SharedBluetoothDevicePairing);
                if (updatedPairing != null) {
                    SetValuesInViewModel(updatedPairing);
                }
            };

            RemovePairingCommand = new Command(() =>
            {
                var innerContext = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
                var innerPairing = context
                              .Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);

                if (innerPairing != null) {
                    context.Delete(innerPairing);    
                }
                OnRemovedPairing?.Invoke(this, null);
            });

            ExchangeDataCommand = new Command(() =>
            {
                var btService = ServiceLocator.Instance.Get<IBluetoothService>();
                btService.ExchangeData();
            });
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

        public Command RemovePairingCommand { get; set; }

        public Command ExchangeDataCommand { get; set; }


        public event EventHandler OnRemovedPairing;
    }
}
