using System;
using Android.Bluetooth;
using Happimeter.Core.Helper;
using Java.Util;
using Happimeter.Core.Models.Bluetooth;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Reactive.Linq;
namespace Happimeter.Watch.Droid.Bluetooth
{
	public class HappimeterPreSurveyDataCharacteristic : BluetoothGattCharacteristic, IWritableCharacteristic
	{
		private const GattProperty GattProperties = GattProperty.Write | GattProperty.Notify;
		private const GattPermission GattPermissions = GattPermission.Write | GattPermission.Read;

		public HappimeterPreSurveyDataCharacteristic() : base(uuid: UUID.FromString(UuidHelper.PreSurveyDataCharacteristicUuidString), properties: GattProperties, permissions: GattPermissions)
		{
			var uuid = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
			var configDescriptor = new BluetoothGattDescriptor(uuid, GattDescriptorPermission.Read | GattDescriptorPermission.Write);
			AddDescriptor(configDescriptor);
			OnWriteReceivedSubject = new Subject<BaseBluetoothMessage>();
			OnWriteReceived = OnWriteReceivedSubject;
		}

		public IObservable<BaseBluetoothMessage> OnWriteReceived { get; set; }
		private Subject<BaseBluetoothMessage> OnWriteReceivedSubject { get; set; }
		public void HandleWriteJson(string json, string deviceAddress)
		{
			PreSurveySecondMessage message = null;
			try
			{
				message = Newtonsoft.Json.JsonConvert.DeserializeObject<PreSurveySecondMessage>(json);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
				return;
			}

			if (message == null)
			{
				return;
			}
			OnWriteReceivedSubject.OnNext(message);
		}

	}
}
