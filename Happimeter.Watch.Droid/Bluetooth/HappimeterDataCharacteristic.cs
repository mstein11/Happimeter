using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth;
using Happimeter.Core.Helper;
using Happimeter.Core.Models.Bluetooth;
using Happimeter.Watch.Droid.ServicesBusinessLogic;
using Happimeter.Watch.Droid.Workers;
using Java.Util;

namespace Happimeter.Watch.Droid.Bluetooth
{
	public class HappimeterDataCharacteristic : BluetoothGattCharacteristic
	{
		private const GattProperty GattProperties = GattProperty.Read | GattProperty.Write | GattProperty.Notify;
		private const GattPermission GattPermissions = GattPermission.Read | GattPermission.Write;

		private Dictionary<string, bool> DidSendPass = new Dictionary<string, bool>();

		private Dictionary<string, ReadHostContext> ReadHostContextForDevice = new Dictionary<string, ReadHostContext>();


		public HappimeterDataCharacteristic() : base(uuid: UUID.FromString(UuidHelper.DataExchangeCharacteristicUuidString), properties: GattProperties, permissions: GattPermissions)
		{
			var uuid = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
			//this.WriteType = GattWriteType.Default;
			var configDescriptor = new BluetoothGattDescriptor(uuid, GattDescriptorPermission.Read | GattDescriptorPermission.Write);
			AddDescriptor(configDescriptor);
		}

		public ReadHostContext GetReadHostContext(string address)
		{
			if (ReadHostContextForDevice.ContainsKey(address))
			{
				//already has a readhostcontext, just go on
				return ReadHostContextForDevice[address];
			}
			if (!DidSendPass.ContainsKey(address))
			{
				//did not greet first!
				return null;
			}
			var toTransfer = ServiceLocator.Instance.Get<IMeasurementService>().GetMeasurementsForDataTransfer();
			ReadHostContextForDevice.Add(address, new ReadHostContext(toTransfer));
			return ReadHostContextForDevice[address];
		}

		public void DoneReading(string address)
		{
			DidSendPass.Remove(address);
			//we do not remove the readhostcontext here, because we will need it during the next write in order to know which things to delete!
		}

		public void HandleWriteJson(string json, string deviceAddress)
		{
			var message = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseBluetoothMessage>(json);


			//initiate auth process
			if (message.MessageName == DataExchangeFirstMessage.MessageNameConstant)
			{
				System.Diagnostics.Debug.WriteLine($"Device {deviceAddress} authenticated with passphrase");
				if (!DidSendPass.ContainsKey(deviceAddress))
				{
					DidSendPass.Add(deviceAddress, true);
				}
				ResetStateForDevice(deviceAddress);
			}

			if (message.MessageName == DataExchangeConfirmationMessage.MessageNameConstant && ReadHostContextForDevice.ContainsKey(deviceAddress))
			{
				var messageToDelete = ReadHostContextForDevice[deviceAddress].Message as DataExchangeMessage;
				ServiceLocator.Instance.Get<IMeasurementService>().DeleteSurveyMeasurement(messageToDelete);
				ResetStateForDevice(deviceAddress);
				if (DidSendPass.ContainsKey(deviceAddress))
				{
					DidSendPass.Remove(deviceAddress);
				}
			}
		}

		private void ResetStateForDevice(string address)
		{

			if (ReadHostContextForDevice.ContainsKey(address))
			{
				ReadHostContextForDevice.Remove(address);
			}
		}
	}
}
