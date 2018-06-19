using System;
using System.Threading.Tasks;
using Plugin.BluetoothLE;

namespace Happimeter.Interfaces
{
	public interface IDeviceInformationService
	{
		string GetPhoneOs();
		string GetDeviceName();
		Task RunCodeInBackgroundMode(Func<Task> action, string name = "MyBackgroundTaskName");
		bool IsIos();
		bool IsAndroid();
	}
}