using Happimeter.Core.Models.Bluetooth;

namespace Happimeter.Watch.Droid.ServicesBusinessLogic
{
    public interface IDeviceService
    {
        string GetDeviceName();
        void AddPairing(AuthSecondMessage message);
        void RemovePairing();
        bool IsPaired();
        void NavigateToPairingRequestPage(string deviceName);
        int? GetMeasurementMode();
        bool IsContinousMeasurementMode();
        void SetBatterySaferMeasurementMode(int measurementInterval = 600);
        void SetContinousMeasurementMode();
    }
}