using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Happimeter.Events;
using Plugin.BluetoothLE;
using Happimeter.Interfaces;

namespace Happimeter.Services
{
	public interface IBluetoothService1 : IBluetoothService
	{
		Task Init();
		IObservable<object> ConnectDevice(IDevice device);
		void WhenConnectionStatusChanged(ConnectionStatus status, IDevice device);
		void ReleaseSubscriptions();
		void UnpairConnection();

		IObservable<IScanResult> StartScan(string serviceGuid = null);
		ReplaySubject<IScanResult> ScanReplaySubject { get; }
		event EventHandler<AndroidWatchExchangeDataEventArgs> DataExchangeStatusUpdate;

		void ExchangeData();
		Task SendGenericQuestions(Action<BluetoothWriteEvent> statusUpdate = null);
		Task SendMeasurementMode(int? interval = null, Action<BluetoothWriteEvent> statusUpdate = null);

		ReplaySubject<(string, string)> NotificationSubject { get; set; }
		ReplaySubject<IGattCharacteristic> CharacteristicsReplaySubject { get; set; }
	}
}