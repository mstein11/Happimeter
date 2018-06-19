using System;
using System.Threading;
using Happimeter.Models;
using System.Linq;
using Happimeter.Core.Helper;
using System.Collections.Generic;

namespace Happimeter.ViewModels.Forms
{
	public class BluetoothPairingItemViewModel : BaseViewModel
	{
		/// <summary>
		///  Just for displaying the right indication on pairing screen
		/// </summary>
		private List<BluetoothDevice> DevicesWithSameName = new List<BluetoothDevice>();

		public BluetoothPairingItemViewModel(BluetoothDevice device)
		{
			UpdateModel(device);
		}

		private BluetoothDevice _device;
		public BluetoothDevice Device
		{
			get => _device;
			set => SetProperty(ref _device, value);
		}

		private string _name;
		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}

		private string _description;
		public string Description
		{
			get => _description;
			set => SetProperty(ref _description, value);
		}
		private string _indicationText;
		public string IndicationText
		{
			get => _indicationText;
			set => SetProperty(ref _indicationText, value);
		}

		private bool _showIndication;
		public bool ShowIndication
		{
			get => _showIndication;
			set => SetProperty(ref _showIndication, value);
		}

		private bool _isUnavailable;
		public bool IsUnavailable
		{
			get => _isUnavailable;
			set => SetProperty(ref _isUnavailable, value);
		}

		private string _isUnavailableReason;

		public string IsUnvailableReason
		{
			get => _isUnavailableReason;
			set => SetProperty(ref _isUnavailableReason, value);
		}

		public void UpdateModel(BluetoothDevice device)
		{
			if (device.ServiceUuids.Any(x => x == UuidHelper.AndroidWatchAuthServiceUuid
										|| x == UuidHelper.AndroidWatchServiceUuid))
			{
				//if the found device has happimeter services
				var alreadyFoundHappimeter = DevicesWithSameName
					.FirstOrDefault(x => x.ServiceUuids.Any(y => y == UuidHelper.AndroidWatchAuthServiceUuid
																		   || y == UuidHelper.AndroidWatchServiceUuid));
				if (alreadyFoundHappimeter != null)
				{
					DevicesWithSameName.Remove(alreadyFoundHappimeter);
				}
			}
			DevicesWithSameName.Add(device);
			Device = device;
			Name = device.Device.Name;
			if (DevicesWithSameName.Any(x => x.ServiceUuids != null
										&& !x.ServiceUuids.Contains(UuidHelper.AndroidWatchAuthServiceUuid)
										&& x.ServiceUuids.Contains(UuidHelper.AndroidWatchServiceUuid)))
			{
				//if we have one in pared mode, show it
				IsUnvailableReason = "Not Available";
				IsUnavailable = true;
			}
			else if (DevicesWithSameName.All(x => x.ServiceUuids != null
											 && !x.ServiceUuids.Contains(UuidHelper.AndroidWatchAuthServiceUuid)
											 && !x.ServiceUuids.Contains(UuidHelper.AndroidWatchServiceUuid)))
			{
				//if all devices have no happimeter service Uuids, there is something wrong
				IsUnvailableReason = "Restart Watch";
				IsUnavailable = true;
			}
			else
			{
				IsUnavailable = false;
			}
		}

		public void ShowIndicationForState(AndroidWatchConnectingStates state)
		{
			switch (state)
			{
				case AndroidWatchConnectingStates.Connecting:
					DisplayIndication("Connecting");
					break;
				case AndroidWatchConnectingStates.BtConnected:
					DisplayIndication("Connected");
					break;
				case AndroidWatchConnectingStates.AuthCharacteristicDiscovered:
					DisplayIndication("Auth discovered");
					break;
				case AndroidWatchConnectingStates.WaitingForNotification:
					DisplayIndication("Awaiting Response");
					break;
				case AndroidWatchConnectingStates.UserAccepted:
					DisplayIndication("User Accepted");
					break;
				case AndroidWatchConnectingStates.UserDeclined:
					DisplayIndication("User Declined", 2000);
					break;
				case AndroidWatchConnectingStates.IsBusy:
					DisplayIndication("Busy, try again or restart app", 2000);
					break;
				case AndroidWatchConnectingStates.SecondWriteSuccessfull:
					DisplayIndication("Wrote twice");
					break;
				case AndroidWatchConnectingStates.Complete:
					DisplayIndication("Pairing successful", 2000);
					break;
				case AndroidWatchConnectingStates.ErrorOnBtConnection:
					DisplayIndication("Not in range or restart phone", 2000);
					break;
				case AndroidWatchConnectingStates.ErrorOnAuthCharacteristicDiscovered:
					DisplayIndication("Error: Restart App on Watch?", 2000);
					break;
				case AndroidWatchConnectingStates.ErrorOnFirstWrite:
					DisplayIndication("Error: Could not write", 2000);
					break;
				case AndroidWatchConnectingStates.NotificationTimeout:
					DisplayIndication("No Response from Watch - abort", 2000);
					break;
				case AndroidWatchConnectingStates.ErrorOnSecondWrite:
					DisplayIndication("Error: Could not write again", 2000);
					break;
				case AndroidWatchConnectingStates.ErrorBeforeComplete:
					DisplayIndication("Error: Sth with db", 2000);
					break;

			}
		}

		private void DisplayIndication(string text, int? milliseconds = null)
		{

			ShowIndication = true;
			IndicationText = text;
			Timer timer = null;

			if (milliseconds != null)
			{
				timer = new Timer((obj) =>
				{
					IndicationText = text;
					ShowIndication = false;
					timer.Dispose();
				}, null, milliseconds.Value, System.Threading.Timeout.Infinite);
			}
		}
	}
}
