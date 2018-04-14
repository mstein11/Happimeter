using System;
using System.Threading;
using Happimeter.Models;

namespace Happimeter.ViewModels.Forms
{
    public class BluetoothPairingItemViewModel : BaseViewModel
    {
        public BluetoothPairingItemViewModel(BluetoothDevice device)
        {
            Device = device;
            Name = device.Device.Uuid.ToString();
            Description = device.Device.Name;
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
                case AndroidWatchConnectingStates.SecondWriteSuccessfull:
                    DisplayIndication("Wrote twice");
                    break;
                case AndroidWatchConnectingStates.Complete:
                    DisplayIndication("Pairing successful", 2000);
                    break;
                case AndroidWatchConnectingStates.ErrorOnBtConnection:
                    DisplayIndication("Error: Device not in Range?", 2000);
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
