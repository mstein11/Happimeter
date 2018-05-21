using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Plugin.BluetoothLE;

namespace Happimeter.Services
{
	public interface IBluetoothService1
	{

		ReplaySubject<IScanResult> ScanReplaySubject { get; }
		List<IScanResult> FoundDevices { get; }

		IObservable<object> ConnectDevice(IDevice device);
		Task<bool> EnableNotificationsFor(IGattCharacteristic characteristic);
		void ExchangeData();
		Task Init();

		IObservable<IScanResult> StartScan(string serviceGuid = null);
		void WhenConnectionStatusChanged(ConnectionStatus status, IDevice device);
	}
}