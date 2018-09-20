using System;
namespace Happimeter.Events
{
    public enum AndroidWatchExchangeDataStates
    {
        SearchingForDevice,
        DeviceNotFound,
        DeviceConnected,
        CouldNotConnect,
        CharacteristicDiscovered,
        CouldNotDiscoverCharacteristic,
        DidWrite,
        ErrorOnExchange,
        ReadUpdate,
        Complete,
        CompleteNeedsAnotherBatch
    }

    public class AndroidWatchExchangeDataEventArgs : EventArgs
    {
        public AndroidWatchExchangeDataEventArgs()
        {
        }
        public AndroidWatchExchangeDataStates EventType { get; set; }

        public int BytesRead { get; set; }

        public int TotalBytes { get; set; }

        public int BatchesTransferred { get; set; }
    }
}
