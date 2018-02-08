using System;
using Happimeter.Models;
using Plugin.BluetoothLE;

namespace Happimeter.Interfaces
{
    public interface IBluetoothService
    {
        bool IsConnected(IDevice device);
        IObservable<IScanResult> StartScan(string serviceGuid = null);
        void ExchangeData();
        IObservable<bool> PairDevice(BluetoothDevice device);
    }
}