using System;
namespace Happimeter.Events
{
    public enum BluetoothWriteEvent
    {
        Initialized,
        Connected,
        ErrorOnConnectingToDevice,
        Complete,
        ErrorOnWrite
    }
}
