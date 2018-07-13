using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Events;
using Happimeter.Models;
using Plugin.BluetoothLE;
using System.Collections.Generic;

namespace Happimeter.Interfaces
{
	public interface IBluetoothService
	{
		Task Init(bool force = false);
		IObservable<object> ConnectDevice(IDevice device);
		void WhenConnectionStatusChanged(ConnectionStatus status, IDevice device);
		void ReleaseSubscriptions();
		void UnpairConnection();

		IObservable<IScanResult> StartScan(string serviceGuid);
		IObservable<IScanResult> StartScan(List<Guid> serviceGuids = null);
		ReplaySubject<IScanResult> ScanReplaySubject { get; }
		event EventHandler<AndroidWatchExchangeDataEventArgs> DataExchangeStatusUpdate;

		void ExchangeData();
		Task SendGenericQuestions(Action<BluetoothWriteEvent> statusUpdate = null);
		Task SendMeasurementMode(int? interval = null, Action<BluetoothWriteEvent> statusUpdate = null);

		ReplaySubject<(string, string)> NotificationSubject { get; set; }
		ReplaySubject<IGattCharacteristic> CharacteristicsReplaySubject { get; set; }

		Task<bool> WriteAsync(IGattCharacteristic characteristic, BaseBluetoothMessage message);
		Task<string> ReadAsync(IGattCharacteristic characteristic, Action<int, int> statusUpdateAction = null);
	}
}