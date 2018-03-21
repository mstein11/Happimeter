using System;
using System.Threading.Tasks;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Events;
using Happimeter.Models;
using Plugin.BluetoothLE;

namespace Happimeter.Interfaces
{
    public interface IBluetoothService
    {
        bool IsConnected(IDevice device);
        void RemoveAllConnections();
        IObservable<IScanResult> StartScan(string serviceGuid = null);
        void ExchangeData();
        IObservable<bool> PairDevice(BluetoothDevice device);
        event EventHandler<AndroidWatchExchangeDataEventArgs> DataExchangeStatusUpdate;
        void SendGenericQuestions(Action<BluetoothWriteEvent> statusUpdate = null);

        Task<bool> WriteAsync(IGattCharacteristic characteristic, BaseBluetoothMessage message);
        Task<string> ReadAsync(IGattCharacteristic characteristic, Action<int, int> statusUpdateAction = null);
        Task<string> AwaitNotificationAsync(IGattCharacteristic characteristic);
    }
}