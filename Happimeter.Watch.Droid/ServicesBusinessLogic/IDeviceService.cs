using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.Database;
using Happimeter.Core.Models;

namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
    public interface IDeviceService
    {
        string GetDeviceName();
        void AddPairing(AuthSecondMessage message);
        void RemovePairing();
        bool IsPaired();
        void NavigateToPairingRequestPage(string deviceName);
        MeasurementModeModel GetMeasurementMode();
        bool IsContinousMeasurementMode();
        void SetMeasurementMode(int id);
        BluetoothPairing GetBluetoothPairing();
        int BatteryPercent();
        string AppVersion();
    }
}