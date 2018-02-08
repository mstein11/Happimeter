using System.Collections.Generic;

namespace Happimeter.Watch.Droid.Database
{
    public interface IDatabaseContext
    {
        bool AddMicrophoneMeasurement(MicrophoneMeasurement measurement);
        void CreateDatabase();
        void DeleteAllMicrophoneMeasurements();
        IList<MicrophoneMeasurement> GetMicrophoneMeasurements();
        void AddNewPairing(BluetoothPairing newPairing);
        BluetoothPairing GetCurrentBluetoothPairing();
        void DeleteAllBluetoothPairings();
    }
}