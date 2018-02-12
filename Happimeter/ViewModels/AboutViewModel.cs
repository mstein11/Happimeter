using System.Windows.Input;
using Happimeter.Core.Database;

namespace Happimeter
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "Debug Info";

            var dbContext = ServiceLocator.Instance.Get<ISharedDatabaseContext>();
            var model = dbContext.Get<SharedBluetoothDevicePairing>(x => x.IsPairingActive);
            UpdateModel(model);

            dbContext.ModelChanged += (sender, e) => {
                var updateModel = sender as SharedBluetoothDevicePairing;
                //if model changed and pairing is still active
                if (updateModel != null && updateModel.IsPairingActive) {
                    UpdateModel(updateModel);
                }
                //if model changed and pairing is not active anymore we restet the view
                if (updateModel != null && updateModel.Id == Id && !updateModel.IsPairingActive) {
                    UpdateModel(null);
                }
            };

        }

        private void UpdateModel(SharedBluetoothDevicePairing model) {
            Id = model?.Id ?? default(int);
            DeviceName = model?.PairedDeviceName ?? "-";
            IsPaired = model != null ? "yes" : "no";
            PairedAt = model?.PairedAt?.ToString() ?? "-";
            LastDataExchange = model?.LastDataSync?.ToString() ?? "-";
            OnModelChanged();
        }

        public int Id { get; set; }

        string deviceName = string.Empty;
        public string DeviceName
        {
            get { return deviceName; }
            set { SetProperty(ref deviceName, value); }
        }

        string isPaired = string.Empty;
        public string IsPaired
        {
            get { return isPaired; }
            set { SetProperty(ref isPaired, value); }
        }

        string pairedAt = string.Empty;
        public string PairedAt
        {
            get { return pairedAt; }
            set { SetProperty(ref pairedAt, value); }
        }

        string lastDataExchange = string.Empty;
        public string LastDataExchange
        {
            get { return lastDataExchange; }
            set { SetProperty(ref lastDataExchange, value); }
        }

        public ICommand OpenWebCommand { get; }
    }
}
