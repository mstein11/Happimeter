using System;
using Java.Security;
using Happimeter.Core.Models.Bluetooth;
namespace Happimeter.Watch.Droid.Bluetooth
{
	public interface IWritableCharacteristic
	{
		IObservable<BaseBluetoothMessage> OnWriteReceived { get; set; }
	}
}
