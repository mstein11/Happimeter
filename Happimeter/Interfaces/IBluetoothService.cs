using System;
using Plugin.BluetoothLE;

namespace Happimeter.Interfaces
{
    public interface IBluetoothService
    {
        bool IsConnected(IDevice device);
        IObservable<IScanResult> StartScan();
        IObservable<object> PairDevice(IDevice device);
    }
}